using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Data.ModelInterfaces;
using App.Data.ServiceInterfaces;
using App.Data.Utils;

#pragma warning disable CS1591

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace App.Data.Models;

/// <summary>
/// Model representing a Location in the dormitory of Borrowable Entity
/// </summary>
[DisplayName("Location")]
public sealed class Location : ICloneable, IDBModel
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName("Location ID")]
    public int LocationID { get; set; }

    [DisplayName("Floor")] [Required] public int Floor { get; set; }

    [DisplayName("Room")] [Required] public int RoomNum { get; set; }

    [DisplayName("Door")] [Required] public int DoorNum { get; set; }

    [DisplayName("Building")] [Required] public char Building { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public string HumanReadableLoc(ILocalizationService loc) =>
        $"{loc["Location"]}: {LocationID}, {Building} {Floor}/{RoomNum}-{DoorNum}";

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

    public string AsLocationString()
    {
        return $"{Building} {Floor}/{RoomNum}-{DoorNum}";
    }
}