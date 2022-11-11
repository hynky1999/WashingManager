using System.ComponentModel;

namespace PrackyASusarny.Data.Models;

[DisplayName ("Drying Room")]
public class DryingRoom : BorrowableEntity
{

    public new string HumanReadable =>
        $"DR ID: {BorrowableEntityID} at {Location?.HumanReadable}";
}