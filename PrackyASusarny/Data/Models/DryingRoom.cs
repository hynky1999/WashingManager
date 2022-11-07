namespace PrackyASusarny.Data.Models;

public class DryingRoom : BorrowableEntity
{
    public new static string HumanReadableName => "Drying Room";
    public override string Label => $"ID: {BorrowableEntityID} at {Location?.Label}";
}