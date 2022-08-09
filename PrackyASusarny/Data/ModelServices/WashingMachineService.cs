using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data;

public sealed class WashingMachineService: ModelService<WashingMachine>
{
    public WashingMachineService(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
    {
    }

    public async Task<List<WashingMachine>> GetFiltered(int? roomNum, (int, int)? floorRange, int? building)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.WashingMachines.Where(wm =>
            floorRange == null || (wm.Location.Floor >= floorRange.Value.Item1 &&
                                   wm.Location.Floor <= floorRange.Value.Item2));
        return await query.ToListAsync();
    }
}