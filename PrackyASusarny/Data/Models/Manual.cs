using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;

namespace PrackyASusarny.Data.Models;

public class Manual : DBModel
{
    public int ManualID { get; set; }
    public string FileName { get; set; }

    [Required] [MaxLength(10)] public string Name { get; set; }

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