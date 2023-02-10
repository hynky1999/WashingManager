using System.Security.Claims;

namespace App.Auth.Utils;

/// <summary>
/// Helper class for working with claims
/// It defines them and provides methods for working with them
/// </summary>
public static class Claims
{
    /// <summary>
    /// Every user must have this claim as it points to the DB where we store the user data
    /// </summary>
    public static string UserID { get; } = "UserID";

    /// <summary>
    /// Claim which allows to create new users
    /// </summary>
    public static string ManageUsers { get; } = "ManageUsers";

    /// <summary>
    /// Claims which allows to manage borrows
    /// </summary>
    public static string ManageBorrows { get; } = "ManageBorrows";

    /// <summary>
    /// Claim which allows to change the DB and modify it's current state
    /// </summary>
    public static string ManageModels { get; } = "ManageModels";


    /// <summary>
    /// Helper method for getting id out of ClaimPrincipal
    /// </summary>
    /// <param name="user">ClaimPrincial representing user</param>
    /// <returns>ID of user data in DB</returns>
    /// <exception cref="ArgumentException">Throws if user not logged in</exception>
    public static int GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(Claims.UserID);
        if (claim == null)
        {
            throw new ArgumentException("User not logged in");
        }

        return int.Parse(claim.Value);
    }
}