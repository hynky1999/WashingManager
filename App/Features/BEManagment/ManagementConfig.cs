using App.Data.Models;

namespace App.Features.BEManagment;

/// <summary>
/// Defines which models can be managed by the BEManagment feature.
/// Must be children of the <see cref="BorrowableEntity"/>
/// </summary>
public static class ManagementConfig
{
    /// <summary>
    /// Allowed models to be managed by the BEManagment feature.
    /// </summary>
    public static readonly Type[] AllowedEntities =
        {typeof(WashingMachine), typeof(DryingRoom)};
}