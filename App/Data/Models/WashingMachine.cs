using System.ComponentModel;
using App.Data.ModelInterfaces;
using App.Data.ServiceInterfaces;
using App.Data.Utils;

#pragma warning disable CS1591

namespace App.Data.Models;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode
/// <summary>
/// Model representing WashingMachine which is Borrowable Entity
/// </summary>
[DisplayName("Washing Machine")]
public sealed class WashingMachine : BorrowableEntity, IDBModel
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [DisplayName("Manual ID")]
    public int? ManualID { get; set; }

    [DisplayName("Manual")] public Manual? Manual { get; set; }

    [DisplayName("Manufacturer")] public string? Manufacturer { get; set; }

    public new string HumanReadableLoc(ILocalizationService loc) =>
        $"{loc["WM"]}: {BorrowableEntityID} {loc["by"]} {loc[Manufacturer]} {loc["at"]} {Location?.HumanReadableLoc(loc)}";

    public override bool Equals(object? obj)
    {
        var wm = obj as WashingMachine;
        return wm is not null && wm.BorrowableEntityID == BorrowableEntityID;
    }

    public override int GetHashCode()
    {
        return BorrowableEntityID;
    }

    public new object Clone()
    {
        var wm = (WashingMachine) MemberwiseClone();
        wm.Manual = Manual?.Clone() as Manual;
        return wm;
    }
}