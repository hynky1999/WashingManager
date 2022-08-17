using System.ComponentModel.DataAnnotations;

namespace PrackyASusarny.Data.Models;

public class WashingMachineStatistics
{
    [Key] public int WashingMachineStatisticsId { get; set; }

    [Required] public WashingMachine WashingMachine { get; set; }

    [Required] public DateTime Date { get; set; }

    public bool[] PerHourAvailable { get; set; }
    public int[] PerHourAvailableCumulative { get; set; }

    [Required] public int CumulativeInt { get; set; }
}