using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

public class Manual : DbModel
{
    [Key] public int ManualID { get; set; }

    [Required] public string FileName { get; set; } = null!;

    [Required] [MaxLength(40)] public string? Name { get; set; }

    public override bool Equals(object? obj)
    {
        var man = obj as Manual;
        return man is not null && man.ManualID == ManualID;
    }

    public override int GetHashCode()
    {
        return ManualID.GetHashCode();
    }

    public override string ToString()
    {
        return FileName;
    }
}