using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Auth.Utils;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Data.EFCoreServices;

public class UserService : IUserService
{
    public readonly ICurrencyService _currencyService;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IRates _rates;

    public UserService(
        IDbContextFactory<ApplicationDbContext> dbFactory, IRates rates,
        ICurrencyService currencyService)
    {
        _dbFactory = dbFactory;
        _rates = rates;
        _currencyService = currencyService;
    }

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

    public async Task<Money> ModifyUserCashAsync(int id, Money money)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        money = await ModifyUserCashAsync(db, id, money);
        await db.SaveChangeAsyncRethrow();
        return money;
    }

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