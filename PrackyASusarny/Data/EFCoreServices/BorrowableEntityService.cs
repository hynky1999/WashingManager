using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Data.EFCoreServices;

public class BorrowableEntityService : IBorrowableEntityService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public BorrowableEntityService(
        IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task ChangeStatus(BorrowableEntity be, Status status)
    {
        var beC = (BorrowableEntity) be.Clone();
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        dbContext.Attach(beC);
        beC.Status = status;
        await dbContext.SaveChangeAsyncRethrow();
    }
}