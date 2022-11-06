namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ILocationService
{
    public int[] GetFloorOptions();
    public char[] GetBuildingOptions();
    public int[] GetRoomOptions();
}