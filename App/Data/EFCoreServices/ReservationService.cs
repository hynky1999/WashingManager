using System.Security.Claims;
using AntDesign.TableModels;
using App.Auth.Utils;
using App.Data.Constants;
using App.Data.Models;
using App.Data.ServiceInterfaces;
using App.Data.Utils;
using App.Middlewares;
using App.Utils;
using Microsoft.EntityFrameworkCore;

namespace App.Data.EFCoreServices;

/// <summary>
/// EF Core implementation of <see cref="IReservationsService"/>.
/// </summary>
public class ReservationService : IReservationsService
{
    private readonly IBorrowService _borrowService;
    private readonly IContextHookMiddleware _contextHookMiddleware;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IClock _clock;
    private readonly IReservationConstant _reservationConstant;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="borrowService"></param>
    /// <param name="reservationConstant"></param>
    /// <param name="contextHookMiddleware"></param>
    /// <param name="clock"></param>
    public ReservationService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IBorrowService borrowService,
        IReservationConstant reservationConstant,
        IContextHookMiddleware contextHookMiddleware, IClock clock)
    {
        _dbFactory = dbFactory;
        _borrowService = borrowService;
        _reservationConstant = reservationConstant;
        _contextHookMiddleware = contextHookMiddleware;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<Reservation?> CreateReservationAsync(Instant start,
        Instant end,
        ClaimsPrincipal userPrincipal, BorrowableEntity entity)
    {
        if (start >= end)
        {
            throw new ArgumentException("Start must be before end");
        }

        var dur = end - start;
        if (dur >
            _reservationConstant.MaxReservationDur)
        {
            throw new ArgumentException("Reservation too long");
        }

        if (dur < _reservationConstant.MinReservationDur)
        {
            throw new ArgumentException("Reservation too short");
        }


        var id = Claims.GetUserId(userPrincipal);
        await using var db = await _dbFactory.CreateDbContextAsync();

        if (start < _clock.GetCurrentInstant() +
            _reservationConstant.MinDurBeforeReservation)
        {
            throw new ArgumentException("Reservation too soon");
        }

        var usersReservations = await GetUserUpcomingReservationsCount(id);
        if (usersReservations >= _reservationConstant.MaxReservationsAtTime)
        {
            throw new ArgumentException("Too many reservations");
        }

        if (entity.Status == Status.Broken)
        {
            throw new ArgumentException("Item is broken");
        }

        var intersection = db.Reservations.FirstOrDefault(r =>
            r.BorrowableEntityID == entity.BorrowableEntityID &&
            r.Start < end && r.End > start);

        if (intersection != null)
        {
            throw new ArgumentException("Reservation intersects with another");
        }


        var reservation = new Reservation
        {
            Start = start,
            End = end,
            BorrowableEntity = entity,
            UserID = id
        };

        // Add only 
        db.Attach(reservation.BorrowableEntity);
        await db.Reservations.AddAsync(reservation);
        await db.SaveChangeAsyncRethrow();
        // Fire and forget
        _contextHookMiddleware.OnSave(EntityState.Added, reservation)
            .FireAndForget();
        return reservation;
    }

    /// <inheritdoc />
    public async Task<Borrow?> MakeBorrowFromReservationAsync(
        Reservation reservation)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        BorrowableEntity? be = reservation.BorrowableEntity;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (be == null)
        {
            var entity = await db.Set<BorrowableEntity>()
                .FirstOrDefaultAsync(beTMP =>
                    beTMP.BorrowableEntityID == reservation.BorrowableEntityID);

            be = entity ??
                 throw new ArgumentException("BorrowableEntity not found");
        }
        var name = reservation.User.UserName ?? "Unknown";
        var surname = reservation.User.UserName ?? "Unknown";

        Borrow borrow = new()
        {
            Start = _clock.GetCurrentInstant(),
            BorrowableEntity = be,
            Reservation = reservation,
            BorrowPerson = new BorrowPerson()
            {
                BorrowPersonID = 0,
                Name = name,
                Surname = surname,
                UserID = reservation.UserID
            }
        };
        return await _borrowService.AddBorrowAsync(borrow);
    }

    /// <inheritdoc />
    public async Task CancelReservationAsync(Reservation reservation)
    {
        if (reservation.Start < _clock.GetCurrentInstant() +
            _reservationConstant.MinReservationCancelDur)
        {
            throw new ArgumentException("Reservation too soon");
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Reservations.Remove(reservation);
        await db.SaveChangeAsyncRethrow();
        _contextHookMiddleware.OnSave(EntityState.Deleted, reservation)
            .FireAndForget();
    }

    /// <inheritdoc />
    public async Task<Reservation[]> GetReservationsAsync(ClaimsPrincipal user,
        QueryModel<Reservation> queryModel)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = GetReservationsByUserQuery(db, user);
        var filtered = queryModel.ExecuteQuery(query);
        return await queryModel.CurrentPagedRecords(filtered).ToArrayAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetReservationsCountAsync(ClaimsPrincipal user,
        QueryModel<Reservation> queryModel)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = GetReservationsByUserQuery(db, user);
        var filtered = queryModel.ExecuteQuery(query);
        return await filtered.CountAsync();
    }

    /// <inheritdoc />
    public async Task<Reservation[]> GetReservationsAsync<T>(
        QueryModel<Reservation> queryModel) where T : BorrowableEntity
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        // ALso get entity because of status 
        var reservations = db.Reservations
            .Include(r => r.BorrowableEntity).ThenInclude(be => be.Location)
            .Include(r => r.User)
            .Include(r => r.BorrowableEntity)
            .Where(r => r.BorrowableEntity is T);

        var filtred = queryModel.ExecuteQuery(reservations);
        return await queryModel.CurrentPagedRecords(filtred).ToArrayAsync();
    }


    /// <inheritdoc />
    public async Task<int> GetReservationsCountAsync<T>(
        QueryModel<Reservation> queryModel) where T : BorrowableEntity
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var reservations = db.Reservations
            .Where(r => r.BorrowableEntity is T);
        var filtred = queryModel.ExecuteQuery(reservations);
        return await filtred.CountAsync();
    }

    /// <inheritdoc />
    public async Task<Reservation[]> GetUpcomingReservationsByEntityAsync(
        BorrowableEntity entity)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var reservation =
            db.Reservations.Where(r => r.Start > _clock.GetCurrentInstant());
        var reservationEntity =
            reservation.Where(r => r.BorrowableEntity == entity);


        return await reservationEntity.ToArrayAsync();
    }

    /// <inheritdoc />
    public async Task<(Instant start, Instant end, T be)[]>
        SuggestReservation<T>(Duration length, int limit = 3)
        where T : BorrowableEntity
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var possibleBes = await db.Set<T>()
            .Where(be => be.Status != Status.Broken).ToArrayAsync();

        var minStart = _clock.GetCurrentInstant() + _reservationConstant.MinDurBeforeReservation;
        var minStartWithOffset = minStart +
                                 _reservationConstant.SuggestReservationDurForBorrow;

        // Use offset as it could be just right now
        var atMinStart =
            await MinStartSuggestReservations(minStartWithOffset, possibleBes,
                length,
                limit);

        // Since the atMinStart will always be the soonest we don't have to search that many anymore.
        possibleBes = possibleBes.Except(atMinStart.Select(t => t.Entity))
            .ToArray();
        limit = Math.Max(limit - possibleBes.Length, 0);

        // InBetween -> There will be some time to create a reservation in between use minStart
        var inBetween = await InBetweenSuggestReservations(minStart,
            possibleBes, length, limit);

        // Ends could be sooner than possible so don't update limit, only possible bes
        // As the time for be inbetween will be always sooner than at the end
        possibleBes = possibleBes.Except(inBetween.Select(t => t.Entity))
            .ToArray();
        var ends =
            await AfterAllSuggestReservations(minStart, possibleBes,
                limit);
        var possibleRes = atMinStart.Concat(inBetween.Concat(ends).ToArray());

        // Group by BE ID and take the one that starts the soonest
        var result = possibleRes.GroupBy(
                r => r.Entity.BorrowableEntityID)
            .Select(g => new {Start = g.Min(x => x.Start), g.First().Entity})
            .Take(limit).OrderBy(x => x.Start).ToArray();


        var projected = result.Select(r =>
        {
            var end = r.Start + length;
            var endLoc = end;
            var startLoc = r.Start;

            return (startLoc, endLoc, r.Entity);
        }).ToArray();


        return projected;
    }

    private async Task<int> GetUserUpcomingReservationsCount(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var now = _clock.GetCurrentInstant();
        var userReservations = await db.Reservations
            .Where(r => r.UserID == id && r.End > now).CountAsync();
        return userReservations;
    }


    private IQueryable<Reservation> GetReservationsByUserQuery(
        ApplicationDbContext db, ClaimsPrincipal user)
    {
        var id = Claims.GetUserId(user);
        return db.Reservations.Include(r => r.User)
            .Include(r => r.BorrowableEntity).ThenInclude(be => be.Location)
            .Where(r => r.UserID == id);
    }

    private async Task<PossibleResStart<T>[]> MinStartSuggestReservations<T>(
        Instant minStart, T[] possibleBes, Duration duration, int limit)
        where T : BorrowableEntity
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        // Get all reservations that end after minStart and take their start if be doesn't have any reservation that ends after minStart then add there dummy with start = minStart + duration
        var bes = from be in db.Set<T>()
            where possibleBes.Contains(be)
            select be;
        var besWithMinStart = from wm in bes
            join r in (from res in db.Reservations
                where res.End > minStart
                select res) on wm.BorrowableEntityID equals r
                .BorrowableEntityID into g
            from subset in g.DefaultIfEmpty()
            select new
            {
                wm, start = subset == null ? minStart + duration : subset.Start
            };


        // Group by be and take the first res start
        var besGrouped = from be in besWithMinStart
            group be by be.wm.BorrowableEntityID
            into g
            select new
            {
                Entity = g.Key,
                Start = g.Min(x => x.start)
            };

        var resEnd = minStart + duration;
        var possible = from be in besGrouped
            where be.Start >= resEnd
            select be.Entity;

        var possibleWithBe = from be in db.Set<T>()
            join p in possible on be.BorrowableEntityID equals p
            select be;

        var withLocalition = possibleWithBe.Include(be => be.Location);

        var possibleRes = await withLocalition.Take(limit).ToArrayAsync();


        return possibleRes.Select(be => new PossibleResStart<T>(minStart, be))
            .ToArray();
    }

    private async Task<PossibleResStart<T>[]> InBetweenSuggestReservations<T>(
        Instant minStart, T[] searchEntities, Duration length, int limit)
        where T : BorrowableEntity
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var futureReservations = db.Reservations.Where(r => r.End > minStart);


        // Gets reservations that end after now for every wm
        // Only applies to non broken BE
        var reservations =
            from wm in db.Set<T>()
            where searchEntities.Contains(wm)
            join r in futureReservations on wm.BorrowableEntityID equals r
                .BorrowableEntityID into g
            from subset in g
            select new
            {
                wm, subset.End, subset.Start
            };


        // Join every reservations with ones that start after it end for same wm
        var crossReservations = from res1 in reservations
            from res2 in reservations
            where res1.wm == res2.wm && res1.End <= res2.Start
            select new {res1, res2};

        // Find next starting reservation after the current one
        var adjacentReservation = from reservation in crossReservations
            group reservation by new
            {
                reservation.res1.Start, reservation.res1.wm.BorrowableEntityID,
                reservation.res1.End
            }
            into g
            select new
            {
                wmID = g.Key.BorrowableEntityID, g.Key.Start, g.Key.End,
                nextResTime = g.Min(r => r.res2.Start)
            };

        // Make sure there is enough time to next reservation
        var availableStarts = from resP in adjacentReservation
            where resP.nextResTime - resP.End >= length
            select new {resP.wmID, resP.End, resP.nextResTime};

        // Gets the time per BE that suffices the length requirement
        var onlyFirstByEntityAvailable = from aS in availableStarts
            group aS by aS.wmID
            into g
            select new {wmId = g.Key, End = g.Min(f => f.End)};

        var ordered = onlyFirstByEntityAvailable.OrderBy(r => r.End);

        // Add wm info
        var withWMInfo = from oF in ordered
            join wm in db.Set<T>().Include(x => x.Location) on oF.wmId equals wm
                .BorrowableEntityID
            select new {wm, oF.End};


        var inbetween = await withWMInfo.Take(limit).ToArrayAsync();
        var inbetweenPosStarts = inbetween.Select(r => new PossibleResStart<T>(
            r.End,
            r.wm)).ToArray();

        return inbetweenPosStarts;
    }

    private async Task<PossibleResStart<T>[]>
        AfterAllSuggestReservations<T>(Instant minStart, T[] possibleBes,
            int limit) where T : BorrowableEntity
    {
        // No need for length here
        await using var db = await _dbFactory.CreateDbContextAsync();

        // Only choose reservations that has something planned as otherwise it was already suggested
        var futureReservations = db.Reservations.Where(r => r.End >= minStart);

        // Same as inbetween, must be here in case of only one reservation which is also end
        var reservations =
            from wm in db.Set<T>()
            where possibleBes.Contains(wm)
            join r in futureReservations on wm.BorrowableEntityID equals r
                .BorrowableEntityID into g
            from subset in g
            select new
            {
                wm, End = subset == null ? minStart : subset.End,
                Start = subset == null ? minStart : subset.Start
            };

        // Choose last one per entity
        var lastReservations = from res in reservations
            group res by res.wm.BorrowableEntityID
            into g
            select new {wmID = g.Key, End = g.Max(r => r.End)};
        // Add wm info
        var withWMInfo = from lR in lastReservations
            join wm in db.Set<T>().Include(x => x.Location) on lR.wmID equals wm
                .BorrowableEntityID
            select new {wm, lR.End};


        var ordered = withWMInfo.OrderBy(r => r.End);
        var ends = await ordered.Take(limit).ToArrayAsync();

        var endsWithMinalStarts = ends.Select(r => new PossibleResStart<T>(
            r.End,
            r.wm)).ToArray();

        return endsWithMinalStarts;
    }

    private record PossibleResStart<T>(Instant Start, T Entity);
}