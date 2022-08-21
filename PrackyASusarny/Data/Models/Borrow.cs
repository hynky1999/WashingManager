using System.ComponentModel.DataAnnotations;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.Utils;

namespace PrackyASusarny.Data.Models;

public class Borrow : DbModel
{
    [UIVisibility(UIVisibilityEnum.Disabled)]
    [Key]
    public int BorrowID { get; set; }

    [Required] public BorrowableEntity BorrowableEntity { get; set; }

    [Required] public BorrowPerson BorrowPerson { get; set; }


    [Required] public DateTime startDate { get; set; }

    public DateTime? endDate { get; set; }

    [UIVisibility(UIVisibilityEnum.Hidden)]
    public uint xmin { get; set; }
}