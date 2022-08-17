using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;

namespace PrackyASusarny.Data.Models;

public sealed class Location : DBModel
{
    [Key] public int LocationID { get; set; }

    [Required] public int Floor { get; set; }

    [Required] public int RoomNum { get; set; }

    [Required] public int DoorNum { get; set; }

    [Required] public char Building { get; set; }

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