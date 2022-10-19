using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

public class Borrow : DbModel
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    public int BorrowID { get; set; }

    [Required] public BorrowableEntity BorrowableEntity { get; set; } = null!;

    [Required] public BorrowPerson BorrowPerson { get; set; } = null!;

    // No access to localiztation service so we have to use this
    [Required] public Instant startDate { get; set; } = SystemClock.Instance.GetCurrentInstant();

    public Instant? endDate { get; set; }

    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }
}