using System.Reflection;
using AntDesign.TableModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Data.EFCoreServices;

public class BorrowService : IBorrowService
{
    private readonly IBorrowPersonService _borrowPersonService;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<BorrowService> _logger;
    private readonly IUsageService _usageService;
    private readonly MethodInfo _usageUpdateStatisticsMethod;

    public BorrowService(IDbContextFactory<ApplicationDbContext> dbFactory,
        IBorrowPersonService borrowPersonService,
        ILocalizationService localizationService, IUsageService usageService,
        ILogger<BorrowService> logger)
    {
        _dbFactory = dbFactory;
        _borrowPersonService = borrowPersonService;
        _localizationService = localizationService;
        _usageService = usageService;
        _usageUpdateStatisticsMethod = typeof(UsageService).GetMethods()
            .FirstOrDefault(m =>
                m.IsGenericMethod && m.Name == "UpdateUsageStatisticsAsync")!;
        _logger = logger;
    }

    public async Task<Price> GetPriceAsync(Borrow borrow)
    {
        var duration = _localizationService.Now - borrow.startDate;
        var price = new Price
        {
            Amount = (int) (duration.Minutes / 30.0 * Rates.WMpricePerHalfHour),
            Currency = "CZK"
        };
        return price;
    }

    public async Task EndBorrowAsync(Borrow borrow)
    {
        var borrowC = (Borrow) borrow.Clone();
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var contextWithStat =
            await EndBorrowStatisticsAsync(borrowC, dbContext);
        contextWithStat.Borrows.Attach(borrowC);
        borrowC.endDate = _localizationService.Now;
        borrowC.BorrowableEntity.Status = Status.Free;
        await contextWithStat.SaveChangeAsyncRethrow();
    }

    public async Task AddBorrowAsync(Borrow borrow)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        // Deepcopy to prevent overwriting original
        var borrowC = (Borrow) borrow.Clone();

        if (borrowC.BorrowableEntity.Status != Status.Free)
            throw new ArgumentException("Invalid Value");

        borrowC.BorrowableEntity.Status = Status.Taken;
        borrowC.startDate = _localizationService.Now;

        if (borrowC.BorrowPerson.BorrowPersonID == 0)
        {
            var id = await _borrowPersonService.GetIdByNameAndSurnameAsync(
                borrowC.BorrowPerson.Name,
                borrowC.BorrowPerson.Surname);
            borrowC.BorrowPerson.BorrowPersonID = id;
        }

        // Register to context
        dbContext.ChangeTracker.TrackGraph(borrowC, CreateBorrowTraversal);
        // Since we use concurency token this will fail if status was modified in meantime.
        await dbContext.SaveChangeAsyncRethrow();
    }

    public async Task<Borrow[]> GetBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity
    {
        using var ctx = _dbFactory.CreateDbContext();
        return await GetBorrowsQuery<T>(ctx, qM).ToArrayAsync();
    }

    public async Task<int> CountBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity
    {
        using var ctx = _dbFactory.CreateDbContext();
        return await GetBorrowsQuery<T>(ctx, qM).CountAsync();
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
        node.Entry.State = EntityState.Unchanged;
        if (node.Entry.Entity is Borrow) node.Entry.State = EntityState.Added;

        if (node.Entry.Entity is BorrowPerson {BorrowPersonID: 0})
            node.Entry.State = EntityState.Added;

        if (node.Entry.Entity is BorrowableEntity)
            node.Entry.Property(nameof(BorrowableEntity.Status)).IsModified =
                true;
    }
}