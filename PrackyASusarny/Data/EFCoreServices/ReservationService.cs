using System.Security.Claims;
using AntDesign.TableModels;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Auth.Utils;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Middlewares;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Data.EFCoreServices;

public class ReservationService : IReservationsService
{
    private readonly IBorrowService _borrowService;
    private readonly IContextHookMiddleware _contextHookMiddleware;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    private readonly ILocalizationService _localizationService;
    private readonly IReservationConstant _reservationConstant;

    public ReservationService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IBorrowService borrowService, ILocalizationService localizationService,
        IReservationConstant reservationConstant,
        IContextHookMiddleware contextHookMiddleware)
    {
        _dbFactory = dbFactory;
        _localizationService = localizationService;
        _borrowService = borrowService;
        _reservationConstant = reservationConstant;
        _contextHookMiddleware = contextHookMiddleware;
    }

    public async Task<Reservation?> CreateReservationAsync(LocalDateTime start,
        LocalDateTime end,
        ClaimsPrincipal userPrincipal, BorrowableEntity entity)
    {
        if (start >= end)
        {
            throw new ArgumentException("Start must be before end");
        }

        var dur = Duration.FromNanoseconds(Period
            .Between(start, end, PeriodUnits.Nanoseconds).Nanoseconds);
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
        using var db = await _dbFactory.CreateDbContextAsync();
        var startInst = start.InZoneLeniently(_localizationService.TimeZone)
            .ToInstant();
        var endInst = end.InZoneLeniently(_localizationService.TimeZone)
            .ToInstant();

        if (startInst < _localizationService.Now +
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
            r.Start < endInst && r.End > startInst);

        if (intersection != null)
        {
            throw new ArgumentException("Reservation intersects with another");
        }


        var reservation = new Reservation
        {
            Start = startInst,
            End = endInst,
            BorrowableEntity = entity,
            UserID = id
        };

        // Add only 
        db.Attach(reservation.BorrowableEntity);
        await db.Reservations.AddAsync(reservation);
        await db.SaveChangeAsyncRethrow();
        // Fire and forget
        _contextHookMiddleware.OnSave(EntityState.Added, reservation);
        return reservation;
    }

    public async Task<Borrow?> MakeBorrowFromReservationAsync(
        Reservation reservation)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        BorrowableEntity? be = reservation.BorrowableEntity;
        if (be == null)
        {
            var entity = await db.Set<BorrowableEntity>()
                .FirstOrDefaultAsync(beTMP =>
                    beTMP.BorrowableEntityID == reservation.BorrowableEntityID);

            if (entity == null)
            {
                throw new ArgumentException("BorrowableEntity not found");
            }

            be = entity;
        }

        Borrow borrow = new()
        {
            startDate = _localizationService.Now,
            BorrowableEntity = be,
            Reservation = reservation,
            BorrowPerson = new BorrowPerson()
            {
                Name = reservation.User.Name,
                Surname = reservation.User.Surname,
            }
        };
        return await _borrowService.AddBorrowAsync(borrow);
    }

    public async Task CancelReservationAsync(Reservation reservation)
    {
        if (reservation.Start < _localizationService.Now +
            _reservationConstant.MinReservationCancelDur)
        {
            throw new ArgumentException("Reservation too soon");
        }

        using var db = await _dbFactory.CreateDbContextAsync();
        db.Reservations.Remove(reservation);
        await db.SaveChangeAsyncRethrow();
        // Fire and forget
        _contextHookMiddleware.OnSave(EntityState.Deleted, reservation);
    }

    public async Task<Reservation[]> GetReservationsAsync(ClaimsPrincipal user,
        QueryModel<Reservation> queryModel)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var query = GetReservationsByUserQuery(db, user);
        var filtered = queryModel.ExecuteQuery(query);
        return await queryModel.CurrentPagedRecords(filtered).ToArrayAsync();
    }

    public async Task<int> GetReservationsCountAsync(ClaimsPrincipal user,
        QueryModel<Reservation> queryModel)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var query = GetReservationsByUserQuery(db, user);
        var filtered = queryModel.ExecuteQuery(query);
        return await filtered.CountAsync();
    }

    public async Task<Reservation[]> GetReservationsAsync<T>(
        QueryModel<Reservation> queryModel) where T : BorrowableEntity
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        // ALso get entity because of status 
        var reservations = db.Reservations
            .Include(r => r.BorrowableEntity).ThenInclude(be => be.Location)
            .Include(r => r.User)
            .Include(r => r.BorrowableEntity)
            .Where(r => r.BorrowableEntity is T);

        var filtred = queryModel.ExecuteQuery(reservations);
        return await queryModel.CurrentPagedRecords(filtred).ToArrayAsync();
    }


    public async Task<int> GetReservationsCountAsync<T>(
        QueryModel<Reservation> queryModel) where T : BorrowableEntity
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var reservations = db.Reservations
            .Where(r => r.BorrowableEntity is T);
        var filtred = queryModel.ExecuteQuery(reservations);
        return await filtred.CountAsync();
    }

    public async Task<Reservation[]> GetUpcomingReservationsByEntityAsync(
        BorrowableEntity entity)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var reservation =
            db.Reservations.Where(r => r.Start > _localizationService.Now);
        var reservationEntity =
            reservation.Where(r => r.BorrowableEntity == entity);


        return await reservationEntity.ToArrayAsync();
    }

    public async Task<(LocalDateTime start, LocalDateTime end, T be)[]>
        SuggestReservation<T>(Duration length, int limit = 3)
        where T : BorrowableEntity
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var possibleRes = await InBetweenSuggestReservations<T>(length, limit);

        // We ends could be sooner than possible
        var ends = await AfterAllSuggestReservations<T>(limit);
        possibleRes = possibleRes.Concat(ends).ToArray();

        var result = possibleRes.GroupBy(
                r => r.Entity.BorrowableEntityID)
            .Select(g => new {Start = g.Min(x => x.Start), g.First().Entity})
            .Take(limit).OrderBy(x => x.Start).ToArray();


        var projected = result.Select(r =>
        {
            var end = r.Start + length;
            var endLoc = end.InZone(_localizationService.TimeZone)
                .LocalDateTime;
            var startLoc = r.Start.InZone(_localizationService.TimeZone)
                .LocalDateTime;

            return (startLoc, endLoc, r.Entity);
        }).ToArray();


        return projected;
    }

    private async Task<int> GetUserUpcomingReservationsCount(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var now = _localizationService.Now;
        var userReservations = await db.Reservations
            .Where(r => r.UserID == id && r.End > now).CountAsync();
        return userReservations;
    }


    private IQueryable<Reservation> GetReservationsByUserQuery(
        ApplicationDbContext db, ClaimsPrincipal user)
    {
        var id = Claims.GetUserId(user);
        return db.Reservations
            .Include(r => r.BorrowableEntity).ThenInclude(be => be.Location)
            .Where(r => r.UserID == id);
    }

    private async Task<PossibleResStart<T>[]> InBetweenSuggestReservations<T>(
        Duration length, int limit)
        where T : BorrowableEntity
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var minStart = _localizationService.Now;
        var minStartWithOffset = minStart +
                                 _reservationConstant.MinDurBeforeReservation;

        // Give some time to borrow
        minStartWithOffset +=
            _reservationConstant.SuggestReservationDurForBorrow;
        var futureReservations = db.Reservations.Where(r => r.End >= minStart);


        // Gets reservations that end after now for every wm
        // If there isn't one such a reservation than fill reservation that starts and ends now
        // Only applies to non broken BE
        var reservations =
            from wm in db.Set<T>()
            where wm.Status != Status.Broken
            join r in futureReservations on wm.BorrowableEntityID equals r
                .BorrowableEntityID into g
            from subset in g.DefaultIfEmpty()
            select new
            {
                wm, End = subset == null ? minStart : subset.End,
                Start = subset == null ? minStart : subset.Start
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

        // Make sure there is enought time to next reservation
        var availableStarts = from resP in adjacentReservation
            where resP.nextResTime - resP.End >= length &&
                  resP.nextResTime - minStartWithOffset >= length
            select new {resP.wmID, resP.End, resP.nextResTime};

        // Gets the time per BE that sufficies the length requirment
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
        var inbetweenWithMinalStarts = inbetween.Select(r =>
        {
            var start = r.End >= minStartWithOffset
                ? r.End
                : minStartWithOffset;
            return new PossibleResStart<T>(start,
                r.wm);
        }).ToArray();

        return inbetweenWithMinalStarts;
    }

    private async Task<PossibleResStart<T>[]>
        AfterAllSuggestReservations<T>(int limit) where T : BorrowableEntity
    {
        // No need for length here
        using var db = await _dbFactory.CreateDbContextAsync();
        var minStart = _localizationService.Now;
        var minStartWithOffset = minStart +
                                 _reservationConstant.MinDurBeforeReservation;
        // Only choose reservations that has something planned as otherwise it was already suggested

        minStartWithOffset +=
            _reservationConstant.SuggestReservationDurForBorrow;
        var futureReservations = db.Reservations.Where(r => r.End >= minStart);

        // Same as inbetween, must be here in case of only one reservatoin which is also end

        var reservations =
            from wm in db.Set<T>()
            where wm.Status != Status.Broken
            join r in futureReservations on wm.BorrowableEntityID equals r
                .BorrowableEntityID into g
            from subset in g.DefaultIfEmpty()
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

        var endsWithMinalStarts = ends.Select(r =>
        {
            var start = r.End >= minStartWithOffset
                ? r.End
                : minStartWithOffset;
            return new PossibleResStart<T>(start,
                r.wm);
        }).ToArray();

        return endsWithMinalStarts;
    }


    private record PossibleResStart<T>(Instant Start, T Entity);
}