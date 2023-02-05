using System.ComponentModel;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.Models;

[DisplayName("Drying Room")]
public class DryingRoom : BorrowableEntity, IDBModel
{
    public new string HumanReadableLoc(ILocalizationService loc) =>
        $"{loc["DR"]}: {BorrowableEntityID} {loc["at"]} {Location?.HumanReadableLoc(loc)}";
}