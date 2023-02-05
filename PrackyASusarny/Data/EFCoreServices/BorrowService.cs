using System.Reflection;
using AntDesign.TableModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Data.EFCoreServices;

public class BorrowService : IBorrowService
{
    private readonly IBorrowPersonService _borrowPersonService;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IRates _rates;
    private readonly IUsageService _usageService;
    private readonly MethodInfo _usageUpdateStatisticsMethod;

    public BorrowService(IDbContextFactory<ApplicationDbContext> dbFactory,
        IBorrowPersonService borrowPersonService,
        ILocalizationService localizationService, IUsageService usageService,
        IRates rates)
    {
        _dbFactory = dbFactory;
        _borrowPersonService = borrowPersonService;
        _localizationService = localizationService;
        _usageService = usageService;
        _rates = rates;
        _usageUpdateStatisticsMethod = typeof(UsageService).GetMethods()
            .FirstOrDefault(m =>
                m.IsGenericMethod && m.Name == "UpdateUsageStatisticsAsync")!;
    }

    public async Task<Money> GetPriceAsync(Borrow borrow)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var end = borrow.End ?? _localizationService.Now;
        var duration = end - borrow.Start;
        var forLength =
            duration.TotalMinutes / 30.0 * _rates.WMpricePerHalfHour;
        var total = forLength + _rates.FlatBorrowPrice;
        var price = new Money
        {
            Amount = (int) total,
            Currency = Currency.CZK
        };
        return price;
    }

    public async Task EndBorrowAsync(Borrow borrow)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();

        var contextWithStat =
            await EndBorrowStatisticsAsync(borrow, dbContext);

        contextWithStat.Borrows.Attach(borrow);
        borrow.End = _localizationService.Now;
        borrow.BorrowableEntity.Status = Status.Free;
        await contextWithStat.SaveChangeAsyncRethrow();
    }

    public async Task<Borrow?> AddBorrowAsync(Borrow borrow)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        if (borrow.BorrowableEntity.Status != Status.Free)
            throw new ArgumentException("Must be free");

        borrow.BorrowableEntity.Status = Status.Taken;
        borrow.Start = _localizationService.Now;

        if (borrow.BorrowPerson.BorrowPersonID == 0)
        {
            // Will create if returns 0 with traversal
            var id = await _borrowPersonService.GetIdByNameAndSurnameAsync(
                borrow.BorrowPerson.Name,
                borrow.BorrowPerson.Surname);
            borrow.BorrowPerson.BorrowPersonID = id;
        }

        // Register to context
        dbContext.ChangeTracker.TrackGraph(borrow, CreateBorrowTraversal);

        // Since we use concurency token this will fail if status was modified in meantime.
        await dbContext.SaveChangeAsyncRethrow();
        return borrow;
    }

    public async Task<Borrow[]> GetBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity
    {
        using var ctx = await _dbFactory.CreateDbContextAsync();
        return await GetBorrowsQuery<T>(ctx, qM).ToArrayAsync();
    }

    public async Task<int> CountBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity
    {
        using var ctx = _dbFactory.CreateDbContext();
        return await GetBorrowsQuery<T>(ctx, qM).CountAsync();
    }

    public async Task<Borrow?> GetBorrowAsync(Reservation res)
    {
        using var ctx = await _dbFactory.CreateDbContextAsync();
        var borrowQuery = from b in ctx.Borrows
            where b.ReservationID == res.ReservationID
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

    public async Task<ApplicationDbContext> EndBorrowStatisticsAsync(
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