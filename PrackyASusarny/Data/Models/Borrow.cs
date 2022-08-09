namespace PrackyASusarny.Data.Models;

public class Borrow
{
    public int BorrowId { get; set; }
    public WashingMachine WashingMachine { get; set;}
    // public PersonalDataModel person { get; set;}
    public DateTime startDate { get; set; }
    public DateTime? endDate { get; set; }
}