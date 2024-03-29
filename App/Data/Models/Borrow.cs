using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Data.ModelInterfaces;
using App.Data.ServiceInterfaces;
using App.Data.Utils;

#pragma warning disable CS1591

// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace App.Data.Models;

/// <summary>
/// Model representing a Borrow of a Borrowable Entity
/// </summary>
[DisplayName("Borrow")]
public class Borrow : IDBModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName("Borrow ID")]
    public int BorrowID { get; set; }


    [DisplayName("Borrowable Entity ID")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int BorrowableEntityID { get; set; }

    [DisplayName("Borrowable Entity")]
    [Required]
    public BorrowableEntity BorrowableEntity { get; set; }


    [DisplayName("Reservation ID")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int? ReservationID { get; set; }

    [DisplayName("Reservation")] public Reservation? Reservation { get; set; }

    // Why keep ? In future it might be possible to make borrow without reservation
    [DisplayName("Borrowable Person ID")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int BorrowPersonID { get; set; }


    [DisplayName("Borrowable Person")]
    [Required]
    public BorrowPerson BorrowPerson { get; set; }

    [DisplayName("Start Time")] [Required] public Instant Start { get; set; }

    [DisplayName("End Time")] public Instant? End { get; set; }

    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }

    public object Clone()
    {
        var borrow = (Borrow) MemberwiseClone();
        borrow.BorrowableEntity = (BorrowableEntity) BorrowableEntity.Clone();
        borrow.BorrowPerson = (BorrowPerson) BorrowPerson.Clone();
        return borrow;
    }

    public string HumanReadableLoc(ILocalizationService loc) =>
        $"{loc["Borrow"]}: {BorrowID} {loc["for"]} {BorrowableEntity.HumanReadableLoc(loc)} {loc["by"]} {BorrowPerson.HumanReadableLoc(loc)}";
}