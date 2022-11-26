using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

public abstract class BorrowableEntity : IDBModel, ICloneable
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName("Borrowable Entity ID")]
    public int BorrowableEntityID { get; set; }

    [DisplayName("Status")] [Required] public Status Status { get; set; }

    [UIVisibility(UIVisibilityEnum.Disabled)]
    [DisplayName("Location ID")]
    public int LocationID { get; set; }

    [DisplayName("Location")] public Location? Location { get; set; }

    // Concurency token
    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }


    public object Clone()
    {
        var be = (BorrowableEntity) MemberwiseClone();
        be.Location = Location?.Clone() as Location;
        return be;
    }

    public string HumanReadableLoc(ILocalizationService loc)
    {
        return loc["Borrowable Entity",
            $"Borrowable Entity ID: {BorrowableEntityID}"] ?? "";
    }
}