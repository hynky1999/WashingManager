using App.Data.Models;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Interface of the service that handles updating the usage data.
/// </summary>
public interface IUsageService
{
    /// <summary>
    /// Update Usage data based on borrow time.
    /// ! IT's is not saved and only updates context.
    /// </summary>
    /// <param name="borrow"></param>
    /// <param name="dbContext"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<ApplicationDbContext> UpdateUsageStatisticsAsync<T>(
        Borrow borrow, ApplicationDbContext dbContext)
        where T : BorrowableEntity;
}