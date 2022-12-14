using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;
#nullable disable

[DisplayName ("Borrow")]
public class Borrow : DbModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName ("Borrow ID")]
    public int BorrowID { get; set; }


    [DisplayName ("Borrowable Entity ID")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int BorrowableEntityID { get; set; }

    [DisplayName ("Borrowable Entity")]
    [Required] public BorrowableEntity BorrowableEntity { get; set; }

    [DisplayName ("Borrowable Person ID")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int BorrowPersonID { get; set; }

    [DisplayName ("Borrowable Person")]
    [Required] public BorrowPerson BorrowPerson { get; set; }

    [DisplayName ("Start Date")]
    [Required] public Instant startDate { get; set; }

    [DisplayName ("End Date")]
    public Instant? endDate { get; set; }

    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }

    public override string HumanReadable =>
        $"Borrow ID: {BorrowID}, {BorrowableEntity.HumanReadable} by {BorrowPerson.HumanReadable}";

    public object Clone()
    {
        var borrow = (Borrow) MemberwiseClone();
        borrow.BorrowableEntity = (BorrowableEntity) BorrowableEntity.Clone();
        borrow.BorrowPerson = (BorrowPerson) BorrowPerson.Clone();
        return borrow;
    }
}