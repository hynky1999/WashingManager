using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Middlewares;
using PrackyASusarny.Utils;

namespace PrackyASusarny.ServerServices;

public class ReservationManager : IReservationManager, IDisposable
{
    private readonly IBorrowService _borrowService;
    private readonly IContextHookMiddleware _contextHookMiddleware;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IRates _rates;
    private readonly IReservationConstant _reservationConstants;
    private readonly ITimedQueueManager<Reservation> _timedQueueManager;
    private readonly IUserService _userService;

    public ReservationManager(IUserService userService,
        ILocalizationService localizationService, IBorrowService borrowService,
        IReservationConstant reservationConstant,
        IRates rates, IContextHookMiddleware middleware,
        IDbContextFactory<ApplicationDbContext> factory)
    {
        _localizationService = localizationService;
        _timedQueueManager = new TimedQueueManager<Reservation>(QueueFactory);
        _userService = userService;
        _borrowService = borrowService;
        _rates = rates;
        _reservationConstants = reservationConstant;
        _contextHookMiddleware = middleware;
        _dbFactory = factory;
    }

    public void Dispose()
    {
        _timedQueueManager.Dispose();
    }

    public async Task<ITimedQueue<Reservation>> Create(int id)
    {
        var queue = _timedQueueManager.CreateQueue(id);
        queue.OnEvent +=
            ProcessEvent;

        await queue.OnChange();
        return queue;
    }

    public void Remove(int id)
    {
        _timedQueueManager.RemoveQueue(id);
    }


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

    public int Count => _timedQueueManager.Count;

    private ITimedQueue<Reservation> QueueFactory(int id)
    {
        return new DBTimedQueue<Reservation>(
            () => GetMostRecentReservationAsync(id),
            (res) => res.End - _localizationService.Now);
    }

    private async Task ResolveNoBorrowAsync(Reservation res)
    {
        await EFExtensions.TryNTimesAsync(async () =>
            await _userService.ModifyUserCashAsync(res.UserID,
                new Money()
                {
                    Amount = -_rates.WMNoBorrowPenalty, Currency = Currency.CZK
                }), 10, 5000);
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
        var newEnd = _localizationService.Now +
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
                      _rates.WMpricePerOverRes;


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
                  r.End <= _localizationService.Now && b.endDate == null
            orderby r.End ascending
            select r;

        var oldReservation = await oldBorrowQuery.FirstOrDefaultAsync();
        if (oldReservation != null)
        {
            return oldReservation;
        }


        // If no old reservation with borrow query the one that ends first.
        var reservationQuery = from r in db.Reservations
            where r.BorrowableEntityID == id && r.End > _localizationService.Now
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
        else if (borrow.endDate == null)
        {
            // Will also update the queue
            await ResolveOverBorrowAsync(reservation);
        }

        //_contextHookMiddleware.OnSave(EntityState.Modified, reservation);
    }
}

public interface IReservationManager
{
    int Count { get; }
    Task<ITimedQueue<Reservation>> Create(int id);
    void Remove(int id);
    Task OnChange(int id);
}