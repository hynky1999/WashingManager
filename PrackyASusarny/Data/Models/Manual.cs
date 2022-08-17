using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;

namespace PrackyASusarny.Data.Models;

public class Manual : DBModel
{
    [Key] public int ManualID { get; set; }

    [Required] public string FileName { get; set; }

    [Required] [MaxLength(40)] public string Name { get; set; }

    public override bool Equals(object? obj)
    {
        var man = obj as Manual;
        return man is not null && man.ManualID == ManualID;
    }

    public override string ToString()
    {
        return FileName;
    }
}