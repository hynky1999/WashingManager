using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class BorrowPersonService : IBorrowPersonService
{
    private readonly IDbContextFactory<ApplicationDbContext> DbFactory;

    public BorrowPersonService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        DbFactory = dbFactory;
    }

    public async Task<int> GetIdByNameAndSurnameAsync(string name, string surname)
    {
        using var dbContext = await DbFactory.CreateDbContextAsync();
        var pQuery = dbContext.BorrowPeople.Where(p => p.Name == name && p.Surname == surname)
            .Select(p => p.BorrowPersonID);
        var personId = await pQuery.FirstOrDefaultAsync();
        return personId;
    }
}