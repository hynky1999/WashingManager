using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class LocationService : CrudService<Location>, ILocationService
{
    public LocationService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<LocationService> logger) : base(
        dbFactory, logger,
        context => context.Locations, (l) => l.LocationID)
    {
    }

    public async Task<List<int>> GetFloorOptionsAsync()
    {
        using var dbContext = await DbFactory.CreateDbContextAsync();
        return await dbContext.Locations.AsNoTracking().Select(x => x.Floor).Distinct().OrderBy(x => x).ToListAsync();
    }

    public async Task<List<char>> GetBuildingOptionsAsync()
    {
        using var dbContext = await DbFactory.CreateDbContextAsync();
        return await dbContext.Locations.AsNoTracking().Select(x => x.Building).Distinct().ToListAsync();
    }
}