using System.Security.Claims;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IUserService
{
    public Task<(string name, string surname)?> GetNameAndSurname(
        ClaimsPrincipal user);
}