using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

namespace PrackyASusarny.Data.Models;
#nullable disable
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

public class Reservation : IDbModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName ("Reservation ID")]
    public int ReservationID { get; set; }
    
    
    [DisplayName ("Borrowable Entitity Id")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int BorrowableEntityID { get; set; }
    
    [DisplayName ("Borrowable Entity")]
    [Required] public BorrowableEntity BorrowableEntity { get; set; }
    
    
    
    [DisplayName ("User ID")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int UserID { get; set; }
    
    [DisplayName ("User")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Required] public User User { get; set; }
    
    
    [DisplayName ("Start Time")]
    [Required]
    public Instant Start { get; set; }
    
    [DisplayName ("End Time")]
    [Required]
    public Instant End { get; set; }
    
    public uint xmin { get; set; }

    public string HumanReadable =>
        $"Reservation ID: {ReservationID}, {BorrowableEntity.HumanReadable} by {User.HumanReadable}";
    
    public object Clone()
    {
        var reservation  = (Reservation) MemberwiseClone();
        reservation.BorrowableEntity = (BorrowableEntity) BorrowableEntity.Clone();
        reservation.User  = (User) User.Clone();
        return reservation;
    }
    
    
}