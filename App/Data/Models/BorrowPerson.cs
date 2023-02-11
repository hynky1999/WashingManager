using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Auth.Models;
using App.Data.ModelInterfaces;
using App.Data.ServiceInterfaces;
using App.Data.Utils;
#pragma warning disable CS1591

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace App.Data.Models;

/// <summary>
/// model for representing a Person that borrows a Borrowable Entity
/// </summary>
[DisplayName("Borrow Person")]
public class BorrowPerson : IDBModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName("Borrow Person ID")]
    public int BorrowPersonID { get; set; }

    [DisplayName("Name")]
    [Required]
    public string Name { get; set; } = "";

    [DisplayName("Surname")]
    [Required]
    public string Surname { get; set; } = "";

    [DisplayName("User ID")]
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int? UserID { get; set; }

    [DisplayName("User")]
    public ApplicationUser? User { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public string HumanReadableLoc(ILocalizationService loc) =>
        $"{loc["Borrow P."]}: {BorrowPersonID}, {Name} {Surname}";
}