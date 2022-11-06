using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class LocationService : ILocationService
{
    private readonly char[] _availableBuildings = {'A', 'B'};
    private readonly int[] _availableFloors = Enumerable.Range(1, 21).ToArray();
    private readonly int[] _availableRooms = Enumerable.Range(1, 21).ToArray();

    public int[] GetFloorOptions()
    {
        return _availableFloors;
    }

    public char[] GetBuildingOptions()
    {
        return _availableBuildings;
    }

    public int[] GetRoomOptions()
    {
        return _availableRooms;
    }
}