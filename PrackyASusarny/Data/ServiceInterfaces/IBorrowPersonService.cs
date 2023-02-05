using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

/// <summary>
/// Service for managing BorrowPerson model
/// </summary>
public interface IBorrowPersonService
{
    public Task<int> GetIdByNameAndSurnameAsync(string name, string surname);
    public Task<BorrowPerson?> GetByIdAsync(int id);
}