using System.ComponentModel.DataAnnotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

public class BorrowPerson
{
    [Key] public int BorrowPersonID { get; set; }

    [Required] public string Name { get; set; } = null!;

    [Required] public string Surname { get; set; } = null!;
}