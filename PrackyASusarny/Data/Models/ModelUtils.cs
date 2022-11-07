namespace PrackyASusarny.Data.Models;

public static class ModelUtils
{
    public static readonly Dictionary<string, Type> ModelNameToType = new()
    {
        {nameof(WashingMachine), typeof(WashingMachine)},
        {nameof(BorrowPerson), typeof(BorrowPerson)},
        {nameof(Location), typeof(Location)},
        {nameof(Manual), typeof(Manual)},
        {nameof(Borrow), typeof(Borrow)},
        {nameof(DryingRoom), typeof(DryingRoom)}
    };
}