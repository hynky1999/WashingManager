namespace PrackyASusarny.Data.Models;

public class Location
{
    public int LocationID { get; set; }
    public int Floor { get; set; }
    public int RoomNum { get; set; }

    public int DoorNum { get; set; }
    public char Building { get; set; }

    public string Print()
    {
        return $"{Building} {Floor}/{RoomNum}-{DoorNum}";
    }
}