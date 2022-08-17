using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

namespace PrackyASusarny.Data.Models;

public enum Status
{
    Taken,
    Broken,
    Free
}

public sealed class WashingMachine : IDbModel
{
    [Key] public int WashingMachineId { get; set; }

    public Manual Manual { get; set; }

    [Required] public Status Status { get; set; }

    public string Manufacturer { get; set; }

    public Location Location { get; set; }

    // Concurency token
    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }

    public override bool Equals(object? obj)
    {
        var wm = obj as WashingMachine;
        return wm is not null && wm.WashingMachineId == WashingMachineId;
    }
}