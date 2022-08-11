using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data;

public class BorrowPersonService: ModelService<BorrowPerson>
{
    
    public BorrowPersonService(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
    {
        
        
    }

    public async Task<int> GetIDByNameAndSurname(string name, string surname)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var pQuery = dbContext.BorrowPeople.Where(p => p.Name == name && p.Surname == surname).Select(p => p.BorrowPersonID);
        var personId = await pQuery.FirstOrDefaultAsync();
        return personId;
    }




}