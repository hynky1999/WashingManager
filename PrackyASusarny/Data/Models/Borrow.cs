using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;
#nullable disable
public class Borrow : DbModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    public int BorrowID { get; set; }


    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int BorrowableEntityID { get; set; }

    [Required] public BorrowableEntity BorrowableEntity { get; set; }

    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int BorrowPersonID { get; set; }

    [Required] public BorrowPerson BorrowPerson { get; set; }

    [Required] public Instant startDate { get; set; }

    public Instant? endDate { get; set; }

    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }

    public override string Label => $"ID: {BorrowID}, {BorrowableEntity.Label} by {BorrowPerson.Label}";

    public object Clone()
    {
        var borrow = (Borrow) MemberwiseClone();
        borrow.BorrowableEntity = (BorrowableEntity) BorrowableEntity.Clone();
        borrow.BorrowPerson = (BorrowPerson) BorrowPerson.Clone();
        return borrow;
    }
}