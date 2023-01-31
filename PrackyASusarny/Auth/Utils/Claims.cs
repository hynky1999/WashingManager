using System.Security.Claims;

namespace PrackyASusarny.Auth.Utils;

public static class Claims
{
    public static string UserID { get; } = "UserID";
    public static string ManageUsers { get; } = "ManageUsers";
    public static string ManageBorrows { get; } = "ManageBorrows";
    public static string ManageModels { get; } = "ManageModels";


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