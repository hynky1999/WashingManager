namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ILocationService
{
    public Task<List<int>> GetFloorOptionsAsync();
    public Task<List<char>> GetBuildingOptionsAsync();
}