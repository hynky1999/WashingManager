using App.Data.Models;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Service for managing BorrowPerson model
/// </summary>
public interface IBorrowPersonService
{
    /// <summary>
    /// Returns a BorrowPerson's id based on the name and surname
    /// </summary>
    /// <param name="name"></param>
    /// <param name="surname"></param>
    /// <returns>id of BP</returns>
    public Task<int> GetIdByNameAndSurnameAsync(string name, string surname);
    

    /// <summary>
    /// Returns Borrow person based on it's id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>BorrowPerson if exists</returns>
    public Task<BorrowPerson?> GetByIdAsync(int id);
}