namespace App.Data.ServiceInterfaces;

/// <summary>
/// Interface for the service that provides data about Locations.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Returns all available floors that can be queried.
    /// </summary>
    /// <returns>Floors</returns>
    public int[] GetFloorOptions();

    /// <summary>
    /// Returns all available Buildings that can be queried.
    /// </summary>
    /// <returns>Buildings</returns>
    public char[] GetBuildingOptions();

    /// <summary>
    /// Returns all available Rooms that can be queried.
    /// </summary>
    /// <returns>Rooms</returns>
    public int[] GetRoomOptions();
}