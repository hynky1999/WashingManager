using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Auth.Models;
using App.Data.ModelInterfaces;
using App.Data.ServiceInterfaces;
using App.Data.Utils;

#pragma warning disable CS1591

// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618

namespace App.Data.Models;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode
/// <summary>
/// Model representing a reservation of Borrowable Entity
/// </summary>
public class Reservation : ICloneable, IDBModel
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName("Reservation ID")]
    public int ReservationID { get; set; }


    [DisplayName("Borrowable Entity ID")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int BorrowableEntityID { get; set; }

    [DisplayName("Borrowable Entity")]
    [Required]
    public BorrowableEntity BorrowableEntity { get; set; }


    [DisplayName("User ID")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    //Claim
    public int UserID { get; set; }


    [DisplayName("User")] [Required] public ApplicationUser User { get; set; }

    [DisplayName("Start Time")] [Required] public Instant Start { get; set; }

    [DisplayName("End Time")] [Required] public Instant End { get; set; }

    public uint xmin { get; set; }

    public object Clone()
    {
        var reservation = (Reservation) MemberwiseClone();
        reservation.BorrowableEntity =
            (BorrowableEntity) BorrowableEntity.Clone();
        return reservation;
    }

    public string HumanReadableLoc(ILocalizationService loc) =>
        $"{loc["Reservation"]}: {ReservationID} {loc["for"]} {BorrowableEntity.HumanReadableLoc(loc)} {loc["by"]} {UserID}";
}