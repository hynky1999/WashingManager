using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.EFCoreServices;

public class LocationService : CrudService<Location>
{
    private IDbContextFactory<ApplicationDbContext> _dbFactory;

    public LocationService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger logger) : base(dbFactory, logger,
        (context) => context.Locations,)
    {
    }

    public async Task<List<int>> GetFloorOptionsAsync()
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Locations.AsNoTracking().Select(x => x.Floor).Distinct().OrderBy(x => x).ToListAsync();
    }

    public async Task<List<char>> GetBuildingOptionsAsync()
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Locations.AsNoTracking().Select(x => x.Building).Distinct().ToListAsync();
    }
}