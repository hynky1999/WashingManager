using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.ServerServices;

public class ReservationHostedService : BackgroundService
{
    private readonly IBorrowableEntityService _borrowableEntityService;
    private readonly IReservationManager _reservationsManager;


    public ReservationHostedService(IReservationManager reservationManager,
        IBorrowableEntityService borrowableEntityService)
    {
        _borrowableEntityService = borrowableEntityService;
        _reservationsManager = reservationManager;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bes = await _borrowableEntityService
            .GetAllBorrowableEntitites<BorrowableEntity>();

        foreach (var be in bes)
        {
            await _reservationsManager.Create(be.BorrowableEntityID);
        }
    }
}