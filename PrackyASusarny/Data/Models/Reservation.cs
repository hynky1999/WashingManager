using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Data.Utils;

namespace PrackyASusarny.Data.Models;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode
public class Reservation : ICloneable, IDBModel
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName("Reservation ID")]
    public int ReservationID { get; set; }


    [DisplayName("Borrowable Entitity Id")]
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
        loc["Reservation",
            $"ID: {ReservationID}, {BorrowableEntity.HumanReadableLoc(loc)} by {UserID}"] ??
        "";
}