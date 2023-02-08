using System.Security.Claims;
using App.Data.Constants;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Interface fo service which manages the user model
/// It takes a Principal which should have a Claim with the user id
/// and then queries the database for the user with that id
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets name and surname of the user
    /// </summary>
    /// <param name="user"></param>
    /// <returns>Name and surname if user found</returns>
    public Task<(string name, string surname)?> GetNameAndSurname(
        ClaimsPrincipal user);

    /// <summary>
    /// Changes a cash of user
    /// </summary>
    /// <param name="id"></param>
    /// <param name="money"></param>
    /// <returns>Changed cash</returns>
    /// <exception cref="ArgumentException">throws if user is not found</exception>
    public Task<Money> ModifyUserCashAsync(int id, Money money);

    /// <summary>
    /// <see cref="ModifyUserCashAsync(int,App.Data.Constants.Money)"/>
    /// Unlike the other method, this just updates provided context
    /// Good for transactions
    /// </summary>
    /// <param name="db"></param>
    /// <param name="id"></param>
    /// <param name="money"></param>
    /// <returns></returns>
    public Task<Money> ModifyUserCashAsync(ApplicationDbContext db, int id,
        Money money);

    /// <summary>
    /// Returns the user's cash
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If user doesn't exist</exception>
    public Task<Money> GetUserCashAsync(ClaimsPrincipal user);
}