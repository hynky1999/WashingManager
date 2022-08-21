using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

namespace PrackyASusarny.Data.Models;

public abstract class BorrowableEntity : DbModel
{
    private static Dictionary<string, Type> _typeCache = new Dictionary<string, Type>()
    {
        {"WashingMachine", typeof(WashingMachine)}
    };

    [Key] public int BorrowableEntityID { get; set; }

    [Required] public Status Status { get; set; }

    public Location Location { get; set; }

    // Concurency token
    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }

    public static string HumanReadableName => "Borrowable Entity";


    public static Type? TypeFactory(string typeName)
    {
        _typeCache.TryGetValue(typeName, out var result);
        return result;
    }
}