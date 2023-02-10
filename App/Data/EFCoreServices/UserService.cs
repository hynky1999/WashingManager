using System.Security.Claims;
using App.Auth.Utils;
using App.Data.Constants;
using App.Data.ServiceInterfaces;
using App.Utils;
using Microsoft.EntityFrameworkCore;

namespace App.Data.EFCoreServices;

/// <summary>
/// EF Core implementation of <see cref="IUserService"/>.
/// </summary>
public class UserService : IUserService
{
    private readonly ICurrencyService _currencyService;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IRates _rates;


    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="rates"></param>
    /// <param name="currencyService"></param>
    public UserService(
        IDbContextFactory<ApplicationDbContext> dbFactory, IRates rates,
        ICurrencyService currencyService)
    {
        _dbFactory = dbFactory;
        _rates = rates;
        _currencyService = currencyService;
    }

    /// <inheritdoc />
    public async Task<(string name, string surname)?> GetNameAndSurname(
        ClaimsPrincipal user)
    {
        var id = user.FindFirst(Claims.UserID);
        if (id == null)
            return null;

        var userId = Claims.GetUserId(user);
        await using var db = await _dbFactory.CreateDbContextAsync();
        var userFound = await db.Users.FindAsync(userId);
        if (userFound == null)
            return null;


        return (userFound.Name, userFound.Surname);
    }

    /// <inheritdoc />
    public async Task<Money> ModifyUserCashAsync(ApplicationDbContext db,
        int id, Money money)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
            throw new ArgumentException("User not found");


        Money dbMoney;
        try
        {
            dbMoney =
                await _currencyService.ConvertToAsync(money, _rates.DBCurrency);
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("Invalid currency");
        }

        user.Cash += dbMoney.Amount;
        return new Money()
        {
            Amount = user.Cash,
            Currency = _rates.DBCurrency
        };
    }

    /// <summary>
    /// Modify user cash
    /// </summary>
    /// <param name="id"></param>
    /// <param name="money"></param>
    /// <returns></returns>
    public async Task<Money> ModifyUserCashAsync(int id, Money money)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        money = await ModifyUserCashAsync(db, id, money);
        await db.SaveChangeAsyncRethrow();
        return money;
    }

    /// <inheritdoc />
    public async Task<Money> GetUserCashAsync(ClaimsPrincipal user)
    {
        var id = Claims.GetUserId(user);
        await using var db = await _dbFactory.CreateDbContextAsync();
        var userFound = await db.Users.FindAsync(id);
        if (userFound == null)
            throw new ArgumentException("User not found");

        return new Money()
        {
            Amount = userFound.Cash,
            Currency = _rates.DBCurrency
        };
    }
}