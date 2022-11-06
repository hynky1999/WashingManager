using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

public class BorrowPerson : DbModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    public int BorrowPersonID { get; set; }

    [Required] public string Name { get; set; } = null!;

    [Required] public string Surname { get; set; } = null!;

    public override string Label => $"ID: {BorrowPersonID}, {Name} {Surname}";

    public object Clone()
    {
        return MemberwiseClone();
    }
}