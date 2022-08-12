namespace PrackyASusarny.Data.Models;

public enum Status
{
    Taken,
    Broken,
    Free
}

public class WashingMachine
{
    public int WashingMachineId { get; set; }
    public Manual Manual { get; set; }
    public Status Status { get; set; }
    public string Manufacturer { get; set; }

    public Location Location { get; set; }

    // Concurency token
    public uint xmin { get; set; }
}