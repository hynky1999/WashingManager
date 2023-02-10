using System.Reflection;
using AntDesign.TableModels;
using App.Data.Constants;
using App.Data.Models;
using App.Data.ServiceInterfaces;
using App.Data.Utils;
using App.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace App.Data.EFCoreServices;

/// <summary>
/// EF Core Implementation of <see cref="IBorrowService"/>.
/// </summary>
public class BorrowService : IBorrowService
{
    private readonly IBorrowPersonService _borrowPersonService;
    private readonly IUserService _userService;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IRates _rates;
    private readonly IUsageService _usageService;
    private readonly MethodInfo _usageUpdateStatisticsMethod;
    private readonly IClock _clock;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="borrowPersonService"></param>
    /// <param name="usageService"></param>
    /// <param name="rates"></param>
    /// <param name="clock"></param>
    /// <param name="userService"></param>
    public BorrowService(IDbContextFactory<ApplicationDbContext> dbFactory,
        IBorrowPersonService borrowPersonService,
        IUsageService usageService,
        IRates rates,
        IClock clock, IUserService userService)
    {
        _dbFactory = dbFactory;
        _borrowPersonService = borrowPersonService;
        _usageService = usageService;
        _rates = rates;
        _clock = clock;
        _userService = userService;
        // We need to decide in runtime which Usage to add to.
        _usageUpdateStatisticsMethod = typeof(UsageService).GetMethods()
            .FirstOrDefault(m =>
                m is
                {
                    IsGenericMethod: true, Name: "UpdateUsageStatisticsAsync"
                })!;
    }

    /// <inheritdoc />
    public async Task<Money> GetPriceAsync(Borrow borrow)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var end = borrow.End ?? _clock.GetCurrentInstant();
        var duration = end - borrow.Start;
        var forLength =
            duration.TotalMinutes / 30.0 * _rates.PricePerHalfHour;
        var total = forLength + _rates.FlatBorrowPrice;
        var price = new Money
        {
            Amount = (int) total,
            Currency = Currency.CZK
        };
        return price;
    }

    /// <inheritdoc />
    public async Task EndBorrowAsync(Borrow borrow, bool deduceUserCash)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();

        var contextWithStat =
            await EndBorrowStatisticsAsync(borrow, dbContext);

        contextWithStat.Borrows.Attach(borrow);
        borrow.End = _clock.GetCurrentInstant();
        borrow.BorrowableEntity.Status = Status.Free;
        if (deduceUserCash)
        {
            var id = borrow.BorrowPerson.UserID;
            if (id == null)
                throw new ArgumentException("User must have UserID");

            await _userService.ModifyUserCashAsync(contextWithStat, id.Value,
                -await GetPriceAsync(borrow));
        }

        await contextWithStat.SaveChangeAsyncRethrow();
    }

    /// <inheritdoc />
    public async Task<Borrow> AddBorrowAsync(Borrow borrow)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        if (borrow.BorrowableEntity.Status != Status.Free)
            throw new ArgumentException("Must be free");

        borrow.BorrowableEntity.Status = Status.Taken;
        borrow.Start = _clock.GetCurrentInstant();

        if (borrow.BorrowPerson.BorrowPersonID == 0)
        {
            // Will create if returns 0 with traversal
            borrow.BorrowPerson.BorrowPersonID = await _borrowPersonService.GetIdByNameAndSurnameAsync(
                borrow.BorrowPerson.Name,
                borrow.BorrowPerson.Surname);
        }

        // Register to context
        dbContext.ChangeTracker.TrackGraph(borrow, CreateBorrowTraversal);

        // Since we use concurrency token this will fail if status was modified in meantime.
        await dbContext.SaveChangeAsyncRethrow();
        return borrow;
    }

    /// <inheritdoc />
    public async Task<Borrow[]> GetBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await GetBorrowsQuery<T>(ctx, qM).ToArrayAsync();
    }

    /// <inheritdoc />
    public async Task<int> CountBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await GetBorrowsQuery<T>(ctx, qM).CountAsync();
    }

    /// <inheritdoc />
    public async Task<Borrow?> GetBorrowAsync(Reservation res)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var borrowQuery = from b in ctx.Borrows
            where b.ReservationID == res.ReservationID
            orderby b.Start descending
            select b;
        
        
        return await borrowQuery.FirstOrDefaultAsync();
    }

    private IQueryable<Borrow> GetBorrowsQuery<T>(
        ApplicationDbContext dbContext, QueryModel<Borrow> qM)
    {
        var filtered = qM.ExecuteQuery(dbContext.Borrows);
        var byType = filtered.Where(b => b.BorrowableEntity is T);
        return byType
            .Include(b => b.BorrowPerson)
            .Include(b => b.BorrowableEntity)
            .ThenInclude(be => be.Location);
    }

    private async Task<ApplicationDbContext> EndBorrowStatisticsAsync(
        Borrow borrow, ApplicationDbContext dbContext)
    {
        // Take dbContext to be able to use it in transaction
        var borrowEntityT = borrow.BorrowableEntity.GetType();
        // Update statistics based on type
        dbContext =
            await (Task<ApplicationDbContext>) _usageUpdateStatisticsMethod
                .MakeGenericMethod(borrowEntityT)
                .Invoke(_usageService, new object[] {borrow, dbContext})!;
        return dbContext;
    }

    private void CreateBorrowTraversal(EntityEntryGraphNode node)
    {
        if (node.Entry.Entity is Borrow) node.Entry.State = EntityState.Added;

        if (node.Entry.Entity is BorrowPerson {BorrowPersonID: 0})
            node.Entry.State = EntityState.Added;

        if (node.Entry.Entity is BorrowableEntity)
            node.Entry.Property(nameof(BorrowableEntity.Status)).IsModified =
                true;
    }
}