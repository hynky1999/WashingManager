using App.Data.Models;
using App.Data.ServiceInterfaces;
using App.Data.Utils;
using App.Utils;
using Microsoft.EntityFrameworkCore;

namespace App.Data.EFCoreServices;

/// <summary>
/// EF Core implementation of <see cref="IBorrowableEntityService"/>.
/// </summary>
public class BorrowableEntityService : IBorrowableEntityService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IReservationsService _reservationsService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="reservationsService"></param>
    public BorrowableEntityService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IReservationsService reservationsService)
    {
        _reservationsService = reservationsService;
        _dbFactory = dbFactory;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<T[]> GetAllBorrowableEntitites<T>()
        where T : BorrowableEntity
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Set<T>().Include(be => be.Location)
            .ToArrayAsync();
    }
}