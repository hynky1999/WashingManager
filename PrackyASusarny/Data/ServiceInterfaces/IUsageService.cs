using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IUsageService
{
    public Task<ApplicationDbContext> UpdateUsageStatisticsAsync<T>(
        Borrow borrow, ApplicationDbContext dbContext)
        where T : BorrowableEntity;
}