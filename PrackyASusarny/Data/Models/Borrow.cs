using PrackyASusarny.Data.ModelInterfaces;

namespace PrackyASusarny.Data.Models;

public class Borrow : DBModel
{
    public int BorrowId { get; set; }
    public WashingMachine WashingMachine { get; set; }

    public BorrowPerson BorrowPerson { get; set; }
    public DateTime startDate { get; set; }
    public DateTime? endDate { get; set; }

    public uint xmin { get; set; }
}