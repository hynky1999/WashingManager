using System.Security.Claims;
using PrackyASusarny.Data.Constants;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IUserService
{
    public Task<(string name, string surname)?> GetNameAndSurname(
        ClaimsPrincipal user);

    public Task<Money> ModifyUserCashAsync(int id, Money money);

    public Task<Money> ModifyUserCashAsync(ApplicationDbContext db, int id,
        Money money);

    public Task<Money> GetUserCashAsync(ClaimsPrincipal user);
}