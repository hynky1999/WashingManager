using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class UserService : IUserService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public UserService(
        IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<(string name, string surname)?> GetNameAndSurname(
        ClaimsPrincipal user)
    {
        var id = user.FindFirst(Claims.UserID);
        if (id == null)
            return null;

        var userId = Claims.GetUserId(user);
        using var db = await _dbFactory.CreateDbContextAsync();
        var userFound = await db.Users.FindAsync(userId);
        if (userFound == null)
            return null;

        return (userFound.Name, userFound.Surname);
    }
}