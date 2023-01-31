using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class BorrowPersonService : IBorrowPersonService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public BorrowPersonService(
        IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

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

    public async Task<BorrowPerson?> GetByIdAsync(int id)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var person = await dbContext.BorrowPeople.FindAsync(id);
        return person;
    }
}