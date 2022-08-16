using PrackyASusarny.Data.ModelInterfaces;

namespace PrackyASusarny.Data.Models;

public sealed class Location : DBModel
{
    public int LocationID { get; set; }
    public int Floor { get; set; }
    public int RoomNum { get; set; }

    public int DoorNum { get; set; }
    public char Building { get; set; }

    public override bool Equals(object? obj)
    {
        var wm = obj as Location;
        return wm is not null && wm.LocationID == LocationID;
    }

    public override string ToString()
    {
        return $"{Building} {Floor}/{RoomNum}-{DoorNum}";
    }
}