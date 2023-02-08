using System.Security.Claims;
using App.Auth.Models;
using Microsoft.AspNetCore.Identity;

namespace App.Auth.Utils;

/// <summary>
/// Auth utils
/// </summary>
public static class Utils
{
    /// <summary>
    /// Creates a user(with password) with userManager and adds claim to it based
    /// on the id it gets from the database
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns>Result of Identity</returns>
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