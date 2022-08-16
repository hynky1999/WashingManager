using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IWashingMachineService : ICrudService<WashingMachine>
{
    Task<List<WashingMachine>> GetFiltered((int, int)? floorRange, char[]? allowedBuildings,
        Status[]? allowedStates);
}