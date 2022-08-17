using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;

namespace PrackyASusarny.Data.Models;

public class Borrow : DBModel
{
    [Key] public int BorrowID { get; set; }

    [Required] public WashingMachine WashingMachine { get; set; }

    [Required] public BorrowPerson BorrowPerson { get; set; }

    [Required] public DateTime startDate { get; set; }

    public DateTime? endDate { get; set; }

    public uint xmin { get; set; }
}