using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.EFCoreServices;

public sealed class WashingMachineService : ModelService<WashingMachine>
{
    public WashingMachineService(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
    {
    }

    public async Task<List<WashingMachine>> GetFiltered((int, int)? floorRange, char[]? allowedBuildings,
        Status[]? allowedStates)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
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