using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PrackyASusarny.Auth.Models;

namespace PrackyASusarny.Auth.Utils;

public static class Utils
{
    public static async Task<IdentityResult> CreateWithClaimAsync(
        this UserManager<ApplicationUser> userManager, ApplicationUser user,
        string password)
    {
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddClaimAsync(user,
                new Claim(Claims.UserID, user.Id.ToString()));
        }

        return result;
    }
}