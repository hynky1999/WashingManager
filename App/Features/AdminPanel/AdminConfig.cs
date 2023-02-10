using App.Data.Models;

namespace App.Features.AdminPanel;

/// <summary>
/// Configuration of the AdminPanel feature.
/// </summary>
public static class AdminConfig
{
    /// <summary>
    /// Defines which models can be managed by the AdminPanel.
    /// </summary>
    public static readonly Type[] AllowedTypes =
    {
        typeof(WashingMachine), typeof(BorrowPerson), typeof(Location),
        typeof(Manual), typeof(DryingRoom), typeof(Reservation), typeof(Borrow)
    };
}