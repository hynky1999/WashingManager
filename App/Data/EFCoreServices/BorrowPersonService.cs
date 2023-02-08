using App.Data.Models;
using App.Data.ServiceInterfaces;
using Microsoft.EntityFrameworkCore;

namespace App.Data.EFCoreServices;

/// <summary>
/// EF Core implementation of <see cref="IBorrowPersonService"/>.
/// </summary>
public class BorrowPersonService : IBorrowPersonService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dbFactory"></param>
    public BorrowPersonService(
        IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <inheritdoc />
    public async Task<int> GetIdByNameAndSurnameAsync(string name,
        string surname)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var pQuery = dbContext.BorrowPeople
            .Where(p => p.Name == name && p.Surname == surname)
            .Select(p => p.BorrowPersonID);
        var personId = await pQuery.FirstOrDefaultAsync();
        return personId;
    }

    /// <inheritdoc />
    public async Task<BorrowPerson?> GetByIdAsync(int id)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var person = await dbContext.BorrowPeople.FindAsync(id);
        return person;
    }
}