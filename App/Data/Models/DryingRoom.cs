using System.ComponentModel;
using App.Data.ModelInterfaces;
using App.Data.ServiceInterfaces;

#pragma warning disable CS1591

namespace App.Data.Models;

/// <summary>
/// Model representing a Drying Room
/// </summary>
[DisplayName("Drying Room")]
public class DryingRoom : BorrowableEntity, IDBModel
{
    public new string HumanReadableLoc(ILocalizationService loc) =>
        $"{loc["DR"]}: {BorrowableEntityID} {loc["at"]} {Location?.HumanReadableLoc(loc)}";
}