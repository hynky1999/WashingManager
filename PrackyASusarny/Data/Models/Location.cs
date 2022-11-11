using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

[DisplayName ("Location")]
public sealed class Location : IDbModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName ("Location ID")]
    public int LocationID { get; set; }

    [DisplayName ("Floor Number")]
    [Required] public int Floor { get; set; }

    [DisplayName ("Room Number")]
    [Required] public int RoomNum { get; set; }

    [DisplayName ("Door Number")]
    [Required] public int DoorNum { get; set; }

    [DisplayName ("Building")]
    [Required] public char Building { get; set; }

    public string HumanReadable =>
        $"Location ID: {LocationID}, {Building} {Floor}/{RoomNum}-{DoorNum}";

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