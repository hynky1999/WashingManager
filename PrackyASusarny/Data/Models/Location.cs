using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

public sealed class Location : DbModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    public int LocationID { get; set; }

    [Required] public int Floor { get; set; }

    [Required] public int RoomNum { get; set; }

    [Required] public int DoorNum { get; set; }

    [Required] public char Building { get; set; }

    public override string Label => $"ID: {LocationID}, {Building} {Floor}/{RoomNum}-{DoorNum}";

    public object Clone()
    {
        return MemberwiseClone();
    }

    public override bool Equals(object? obj)
    {
        var wm = obj as Location;
        return wm is not null && wm.LocationID == LocationID;
    }

    public override int GetHashCode()
    {
        return LocationID;
    }

    public override string ToString()
    {
        return $"{Building} {Floor}/{RoomNum}-{DoorNum}";
    }
}