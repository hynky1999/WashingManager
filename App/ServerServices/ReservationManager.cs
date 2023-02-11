using App.Data;
using App.Data.Constants;
using App.Data.Models;
using App.Data.ServiceInterfaces;
using App.Middlewares;
using App.Utils;
using Microsoft.EntityFrameworkCore;

namespace App.ServerServices;

/// Implementation of IReservationManager />
public class ReservationManager : IReservationManager, IDisposable
{
    private readonly IBorrowService _borrowService;
    private readonly IContextHookMiddleware _contextHookMiddleware;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IRates _rates;
    private readonly IClock _clock;
    private readonly IReservationConstant _reservationConstants;
    private readonly ITimedQueueManager<Reservation> _timedQueueManager;
    private readonly IUserService _userService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="borrowService"></param>
    /// <param name="reservationConstant"></param>
    /// <param name="rates"></param>
    /// <param name="middleware"></param>
    /// <param name="factory"></param>
    /// <param name="clock"></param>
    public ReservationManager(IUserService userService,
        IBorrowService borrowService,
        IReservationConstant reservationConstant,
        IRates rates, IContextHookMiddleware middleware,
        IDbContextFactory<ApplicationDbContext> factory, IClock clock)
    {
        _timedQueueManager = new TimedQueueManager<Reservation>(QueueFactory);
        _userService = userService;
        _borrowService = borrowService;
        _rates = rates;
        _reservationConstants = reservationConstant;
        _contextHookMiddleware = middleware;
        _dbFactory = factory;
        _clock = clock;
    }

    /// <summary>
    /// Disposes all queues
    /// </summary>
    public void Dispose()
    {
        _timedQueueManager.Dispose();
    }

    /// <inheritdoc />
    public async Task<ITimedQueue<Reservation>> Create(int id)
    {
        var queue = _timedQueueManager.CreateQueue(id);
        queue.OnEvent +=
            ProcessEvent;

        await queue.OnChange();
        return queue;
    }

    /// <inheritdoc />
    public void Remove(int id)
    {
        _timedQueueManager.RemoveQueue(id);
    }


    /// <inheritdoc />
    public async Task OnChange(int id)
    {
        var queue = _timedQueueManager.GetQueue(id);
        if (queue == null)
        {
            return;
        }

        try
        {
            await queue.OnChange();
        }
        catch (ObjectDisposedException)
        {
            // Ignore
        }
    }

    /// <inheritdoc />
    public int Count => _timedQueueManager.Count;

    private ITimedQueue<Reservation> QueueFactory(int id)
    {
        return new DBTimedQueue<Reservation>(
            () => GetMostRecentReservationAsync(id),
            (res) => res.End - _clock.GetCurrentInstant());
    }

    private async Task ResolveNoBorrowAsync(Reservation res)
    {
        await EFExtensions.TryNTimesAsync(async () =>
            await _userService.ModifyUserCashAsync(res.UserID,
                new Money()
                {
                    Amount = -_rates.NoBorrowPenalty, Currency = Currency.CZK
                }));
    }

    // Refetch in order to get new xmin
    private async Task ProlongReservations(int resID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var res = await db.Reservations.FindAsync(resID);
        if (res == null)
            return;

        // Postpone all starting after reservation

        var oldEnd = res.End;
        var newEnd = _clock.GetCurrentInstant() +
                     _reservationConstants.ReservationPostponeDur;


        var upcomingResQuery = from r in db.Reservations
            where r.BorrowableEntityID == res.BorrowableEntityID &&
                  r.Start >= oldEnd && r.ReservationID != res.ReservationID
            orderby r.Start
            select r;

        var upcomingRes = await upcomingResQuery.ToArrayAsync();
        var last_end = newEnd;
        foreach (var resM in upcomingRes)
        {
            if (resM.Start >= last_end)
            {
                break;
            }

            var postponeDuration = last_end - resM.Start;
            resM.Start += postponeDuration;
            resM.End += postponeDuration;
            last_end = resM.End;
        }

        // Prolong reservation
        db.Attach(res);
        res.End = newEnd;

        // Penalty
        var penalty = (newEnd - oldEnd) /
                      _reservationConstants.ReservationPostponeDur *
                      _rates.PricePerOverRes;


        await _userService.ModifyUserCashAsync(db, res.UserID,
            new Money() {Amount = -(int) penalty, Currency = Currency.CZK});

        await db.SaveChangeAsyncRethrow();
        // Send just one OnSave event
    }

    private async Task ResolveOverBorrowAsync(Reservation res)
    {
        // Prolong reservation
        await EFExtensions.TryNTimesAsync(async () =>
            await ProlongReservations(res.ReservationID));
    }

    private async Task<Reservation?> GetMostRecentReservationAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        // Try to find reservation that is old and still has a borrow
        var oldBorrowQuery = from r in db.Reservations
            join b in db.Borrows on r.ReservationID equals b.ReservationID
            where r.BorrowableEntityID == id &&
                  r.End <= _clock.GetCurrentInstant() && b.End == null
            orderby r.End ascending
            select r;

        var oldReservation = await oldBorrowQuery.FirstOrDefaultAsync();
        if (oldReservation != null)
        {
            return oldReservation;
        }


        // If no old reservation with borrow query the one that ends first.
        var reservationQuery = from r in db.Reservations
            where r.BorrowableEntityID == id && r.End > _clock.GetCurrentInstant()
            orderby r.End
            select r;
        var futureReservation = await reservationQuery.FirstOrDefaultAsync();
        return futureReservation;
    }

    async void ProcessEvent(object? sender, Reservation reservation)
    {
        var borrow = await _borrowService.GetBorrowAsync(reservation);
        if (borrow == null)
        {
            await ResolveNoBorrowAsync(reservation);
        }

        // Not returned in Reservation time
        else if (borrow.End == null)
        {
            // Will also update the queue
            await ResolveOverBorrowAsync(reservation);
        }

        _contextHookMiddleware.OnSave(EntityState.Modified, reservation).FireAndForget();
    }
}