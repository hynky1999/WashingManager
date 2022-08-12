using System.ComponentModel.DataAnnotations;

namespace PrackyASusarny.Data.Models;

public class BorrowPerson
{
    public int BorrowPersonID { get; set; }

    [Required] public string Name { get; set; }

    [Required] public string Surname { get; set; }
}