using App.Data.Models;

namespace App.Data.Utils;

/// <summary>
/// Converts model name to it's type
/// </summary>
public static class ModelUtils
{
    /// <summary>
    /// Converts model name to it's type
    /// </summary>
    public static readonly Dictionary<string, Type> ModelNameToType = new()
    {
        {nameof(WashingMachine), typeof(WashingMachine)},
        {nameof(BorrowPerson), typeof(BorrowPerson)},
        {nameof(Location), typeof(Location)},
        {nameof(Manual), typeof(Manual)},
        {nameof(Borrow), typeof(Borrow)},
        {nameof(DryingRoom), typeof(DryingRoom)},
        {nameof(Reservation), typeof(Reservation)},
    };
}