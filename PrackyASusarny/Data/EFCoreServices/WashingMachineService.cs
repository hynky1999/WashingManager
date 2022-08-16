using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public sealed class WashingMachineService : CrudService<WashingMachine>, IWashingMachineService
{
    public WashingMachineService(IDbContextFactory<ApplicationDbContext> dbFactory,
        ILogger<WashingMachineService> logger) : base(dbFactory, logger, context => context.WashingMachines,
        (machine) => machine.WashingMachineId)
    {
    }

    public async Task<List<WashingMachine>> GetFiltered((int, int)? floorRange, char[]? allowedBuildings,
        Status[]? allowedStates)
    {
        using var context = await DbFactory.CreateDbContextAsync();
        var query = context.WashingMachines.AsQueryable();
        if (floorRange != null)
        {
            query = query.Where(wm =>
                (wm.Location.Floor >= floorRange.Value.Item1 &&
                 wm.Location.Floor <= floorRange.Value.Item2));
        }

        if (allowedBuildings != null)
        {
            query = query.Where(wm => allowedBuildings.Contains(wm.Location.Building));
        }

        if (allowedStates != null)
        {
            query = query.Where(wm => allowedStates.Contains(wm.Status));
        }

        query = query.Include(wm => wm.Location).Include(wm => wm.Manual);

        return await query.Take(4).ToListAsync();
    }
}