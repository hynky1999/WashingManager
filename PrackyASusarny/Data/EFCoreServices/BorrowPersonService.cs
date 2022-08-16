using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class BorrowPersonService : CrudService<BorrowPerson>, IBorrowPersonService
{
    public BorrowPersonService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<BorrowPersonService> logger) :
        base(dbFactory, logger, context => context.BorrowPeople, (person) => person.BorrowPersonID)
    {
    }

    public async Task<int> GetIDByNameAndSurname(string name, string surname)
    {
        using var dbContext = await DbFactory.CreateDbContextAsync();
        var pQuery = dbContext.BorrowPeople.Where(p => p.Name == name && p.Surname == surname)
            .Select(p => p.BorrowPersonID);
        var personId = await pQuery.FirstOrDefaultAsync();
        return personId;
    }
}