using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Data.EFCoreServices;

public class BorrowableEntityService : IBorrowableEntityService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IReservationsService _reservationsService;

    public BorrowableEntityService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IReservationsService reservationsService)
    {
        _reservationsService = reservationsService;
        _dbFactory = dbFactory;
    }

    public async Task ChangeStatus(BorrowableEntity be, Status status)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        dbContext.Attach(be);
        be.Status = status;
        if (status == Status.Broken)
        {
            var upcomingRes = await _reservationsService
                .GetUpcomingReservationsByEntityAsync(be);
            dbContext.Reservations.RemoveRange(upcomingRes);
        }

        await dbContext.SaveChangeAsyncRethrow();
    }

    public async Task<T[]> GetAllBorrowableEntitites<T>()
        where T : BorrowableEntity
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Set<T>().Include(be => be.Location)
            .ToArrayAsync();
    }
}