namespace PrackyASusarny.Data.Models;

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
}