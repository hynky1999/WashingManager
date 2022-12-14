using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

[DisplayName ("Borrow Person")]
public class BorrowPerson : DbModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName ("Borrow Person ID")]
    public int BorrowPersonID { get; set; }

    [DisplayName ("Name")]
    [Required] public string Name { get; set; } = null!;

    [DisplayName ("Surname")]
    [Required] public string Surname { get; set; } = null!;

    public override string HumanReadable => $"Borrow P. ID: {BorrowPersonID}, {Name} {Surname}";

    public object Clone()
    {
        return MemberwiseClone();
    }
}