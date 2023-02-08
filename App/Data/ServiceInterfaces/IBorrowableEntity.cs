using App.Data.Models;
using App.Data.Utils;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Service for managing BorrowableEntity model and it's descendants
/// </summary>
public interface IBorrowableEntityService
{
    /// <summary>
    /// Change Status property of BE
    /// </summary>
    /// <param name="be">Borrowable Entitiy</param>
    /// <param name="status">Status</param>
    /// <returns></returns>
    public Task ChangeStatus(BorrowableEntity be, Status status);

    /// <summary>
    /// Returns all BEs of certain type
    /// </summary>
    /// <typeparam name="T">Type or Borrowable entity</typeparam>
    /// <returns></returns>
    public Task<T[]> GetAllBorrowableEntitites<T>()
        where T : BorrowableEntity;
}