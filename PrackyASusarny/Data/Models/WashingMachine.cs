namespace PrackyASusarny.Data.Models;

public enum Status
{
    Taken,
    Broken,
    Free
}

public class WashingMachine
{
    public int WashingMachineID { get; set; }
    public Manual Manual {get; set; }
    public int ManualID { get; set; }
    public Status Status { get; set; }
    public string Manufacturer { get; set;}

    public Location Location { get; set;}
    public int LocationID { get; set; }
    
    // Concurency token
    public uint xmin { get; set; }
}