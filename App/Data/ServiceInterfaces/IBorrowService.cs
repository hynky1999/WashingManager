using AntDesign.TableModels;
using App.Data.Constants;
using App.Data.Models;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Service which provides methods for working Borrow model
/// </summary>
public interface IBorrowService
{
    /// <summary>
    /// Computes the total price of the borrow
    /// </summary>
    /// <param name="borrow"></param>
    /// <returns>Price for a borrow</returns>
    Task<Money> GetPriceAsync(Borrow borrow);

    /// <summary>
    /// Ends the borrow and updates the usage statistics
    /// </summary>
    /// <param name="borrow"></param>
    Task EndBorrowAsync(Borrow borrow);

    /// <summary>
    /// Creates a new borrow
    /// </summary>
    /// <param name="borrow"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">throws if BE is not free</exception>
    Task<Borrow?> AddBorrowAsync(Borrow borrow);

    /// <summary>
    /// Returns all borrows for a type of borrowable entity
    /// </summary>
    /// <param name="qM">QueryModel with constraints for query</param>
    /// <typeparam name="T">Type of BE to query </typeparam>
    /// <returns>All borrows for T</returns>
    Task<Borrow[]> GetBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity;

    /// <summary>
    /// <see cref="GetBorrowsByBEAsync{T}"/> but only count
    /// </summary>
    /// <param name="qM"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<int> CountBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity;

    /// <summary>
    /// Gets borrow for a reservation
    /// </summary>
    /// <param name="res"></param>
    /// <returns>If exists borrow for reservation otherwise null</returns>
    Task<Borrow?> GetBorrowAsync(Reservation res);
}