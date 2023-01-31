using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Middlewares;
using PrackyASusarny.ServerServices;

namespace PrackyASusarny.Utils;

public static class ProgramInit
{
    public static void InitializeHooks(IContextHookMiddleware middleware,
        IReservationManager reservationManager)
    {
        // Whatever happens to Reservation inform reservation manager
        foreach (var state in new[]
                 {
                     EntityState.Added, EntityState.Modified,
                     EntityState.Deleted
                 })
        {
            middleware.AddContextHook<Reservation>(state,
                async (entity) =>
                {
                    await reservationManager.OnChange(entity
                        .BorrowableEntityID);
                });
        }

        // BorrowableEntity added create queue
        var beCreateHook = async (BorrowableEntity entity) =>
        {
            await reservationManager.Create(entity.BorrowableEntityID);
        };
        var beRemoveHook = (BorrowableEntity entity) =>
        {
            reservationManager.Remove(entity.BorrowableEntityID);
            return Task.CompletedTask;
        };
        var geAddHook =
            typeof(IContextHookMiddleware).GetMethod(
                nameof(IContextHookMiddleware.AddContextHook))!;
        foreach (var type in new[]
                 {
                     typeof(BorrowableEntity), typeof(WashingMachine),
                     typeof(DryingRoom)
                 })
        {
            // Add and Remove hooks
            geAddHook.MakeGenericMethod(type).Invoke(middleware,
                new object[] {EntityState.Added, beCreateHook});
            geAddHook.MakeGenericMethod(type).Invoke(middleware,
                new object[] {EntityState.Deleted, beRemoveHook});
            // No need for modify hook as there is no way to modify ID of BE
        }
    }

    public async static Task InitializeQueues(
        IReservationManager reservationManager,
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IBorrowableEntityService beService)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var bes = await beService
            .GetAllBorrowableEntitites<BorrowableEntity>();

        foreach (var be in bes)
        {
            await reservationManager.Create(be.BorrowableEntityID);
        }
    }
}