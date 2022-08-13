using System.ComponentModel.DataAnnotations.Schema;
using PrackyASusarny.Data.EFCoreServices;

namespace PrackyASusarny.Data.Models;

public class Borrow : IModelWithService<Borrow>
{
    public Borrow()
    {
    }

    public int BorrowId { get; set; }
    public WashingMachine WashingMachine { get; set; }

    public BorrowPerson BorrowPerson { get; set; }
    public DateTime startDate { get; set; }
    public DateTime? endDate { get; set; }

    public uint xmin { get; set; }

    [NotMapped] public ICrudService<Borrow> Service { get; }
}