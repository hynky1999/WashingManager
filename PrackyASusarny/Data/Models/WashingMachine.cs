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
    public Manual? Manual { get; set; }

    public string? Manufacturer { get; set; }

    public new static string HumanReadableName => "Washing Machine";

    public override bool Equals(object? obj)
    {
        var wm = obj as WashingMachine;
        return wm is not null && wm.BorrowableEntityID == BorrowableEntityID;
    }

    public override int GetHashCode()
    {
        return BorrowableEntityID;
    }
}