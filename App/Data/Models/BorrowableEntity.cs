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
/// Model representing a entity that can be borrowed. It's not a real entity, but a base class for other entities.
/// </summary>
public abstract class BorrowableEntity : IDBModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName("Borrowable Entity ID")]
    public int BorrowableEntityID { get; set; }

    [DisplayName("Status")] [Required] public Status Status { get; set; }

    [UIVisibility(UIVisibilityEnum.Disabled)]
    [DisplayName("Location ID")]
    public int? LocationID { get; set; }

    [DisplayName("Location")] public Location? Location { get; set; }

    // Concurency token
    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }


    public object Clone()
    {
        var be = (BorrowableEntity) MemberwiseClone();
        be.Location = Location?.Clone() as Location;
        return be;
    }

    public string HumanReadableLoc(ILocalizationService loc)
    {
        return $"{loc["Borrowable Entity"]}: {BorrowableEntityID}";
    }
}