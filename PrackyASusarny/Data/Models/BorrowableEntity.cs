using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

public abstract class BorrowableEntity : DbModel
{
    private static readonly Dictionary<string, Type> _typeCache = new()
    {
        {"WashingMachine", typeof(WashingMachine)}
    };

    [Key] public int BorrowableEntityID { get; set; }

    [Required] public Status Status { get; set; }

    public Location? Location { get; set; }

    // Concurency token
    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }

    public new static string HumanReadableName => "Borrowable Entity";


    public static Type? TypeFactory(string typeName)
    {
        _typeCache.TryGetValue(typeName, out var result);
        return result;
    }
}