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

public sealed class WashingMachine : BorrowableEntity
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    public int ManualID { get; set; }

    public Manual? Manual { get; set; }

    public string? Manufacturer { get; set; }

    public new static string HumanReadableName => "Washing Machine";

    public override string Label => $"ID: {BorrowableEntityID} by {Manufacturer} at {Location?.Label}";

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