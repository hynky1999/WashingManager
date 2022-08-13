using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ILocationService : ICrudService<Location>
{
    public Task<List<int>> GetFloorOptionsAsync();
    public Task<List<int>> GetBuildingOptionsAsync();
}