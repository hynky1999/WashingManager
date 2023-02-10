using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Data.ModelInterfaces;
using App.Data.ServiceInterfaces;
using App.Data.Utils;

#pragma warning disable CS1591

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace App.Data.Models;

/// <summary>
/// Model representing a manual for a Borrowable Entity
/// </summary>
[DisplayName("Manual")]
public class Manual : ICloneable, IDBModel
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    [DisplayName("Manual ID")]
    public int ManualID { get; set; }

    [DisplayName("File Name")]
    [Required]
    public string FileName { get; set; } = null!;

    public object Clone()
    {
        return MemberwiseClone();
    }

    public string HumanReadableLoc(ILocalizationService loc) =>
        $"{loc["Manual"]}: {loc[ManualID]}, {loc[FileName]}";

    public override bool Equals(object? obj)
    {
        var man = obj as Manual;
        return man is not null && man.ManualID == ManualID;
    }

    public override int GetHashCode()
    {
        return ManualID.GetHashCode();
    }

    public override string ToString()
    {
        return FileName;
    }
}