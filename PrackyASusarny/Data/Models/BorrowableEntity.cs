using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

public abstract class BorrowableEntity : DbModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    public int BorrowableEntityID { get; set; }

    [Required] public Status Status { get; set; }

    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int LocationID { get; set; }

    public Location? Location { get; set; }

    // Concurency token
    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }

    public new static string HumanReadableName => "Borrowable Entity";

    public object Clone()
    {
        var be = (BorrowableEntity) MemberwiseClone();
        be.Location = Location?.Clone() as Location;
        return be;
    }
}