using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data;

public class LocationService : ModelService<Location>
{
    public LocationService(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
    {
        
        
    }

    public async Task<List<int>> GetFloorOptions()
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Locations.AsNoTracking().Select(x => x.Floor).Distinct().OrderBy(x => x).ToListAsync();
    }

    public async Task<List<char>> GetBuildingOptions()
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Locations.AsNoTracking().Select(x => x.Building).Distinct().ToListAsync();
    }
    
    public async Task<List<int>> GetUniqueRoomNums()
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Locations.AsNoTracking().Select(x => x.RoomNum).Distinct().ToListAsync();
    }

}