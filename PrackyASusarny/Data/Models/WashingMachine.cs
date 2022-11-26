using System.ComponentModel;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Data.Utils;

namespace PrackyASusarny.Data.Models;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode
public enum Status
{
    Taken,
    Broken,
    Free
}

[DisplayName("Washing Machine")]
public sealed class WashingMachine : BorrowableEntity, IDBModel
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [DisplayName("Manual ID")]
    public int ManualID { get; set; }

    [DisplayName("Manual")] public Manual? Manual { get; set; }

    [DisplayName("Manufacturer")] public string? Manufacturer { get; set; }

    public new string HumanReadableLoc(ILocalizationService loc) =>
        loc["WM", "by", Manufacturer, "at", Location?.HumanReadableLoc(loc)] ??
        "";

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