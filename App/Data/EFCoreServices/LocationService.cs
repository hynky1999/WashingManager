using App.Data.ServiceInterfaces;

namespace App.Data.EFCoreServices;

/// <summary>
/// EF Core implementation of <see cref="ILocalizationService"/>.
/// </summary>
public class LocationService : ILocationService
{
    private readonly char[] _availableBuildings = {'A', 'B'};
    private readonly int[] _availableFloors = Enumerable.Range(1, 21).ToArray();
    private readonly int[] _availableRooms = Enumerable.Range(1, 21).ToArray();

    /// <inheritdoc />
    public int[] GetFloorOptions()
    {
        return _availableFloors;
    }

    /// <inheritdoc />
    public char[] GetBuildingOptions()
    {
        return _availableBuildings;
    }

    /// <inheritdoc />
    public int[] GetRoomOptions()
    {
        return _availableRooms;
    }
}