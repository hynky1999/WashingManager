using System.Security.Claims;

namespace PrackyASusarny.Auth.Models;

public static class Claims
{
    public static string UserID { get; } = "UserID";
    public static string ManageUsers { get; } = "ManageUsers";
    public static string ManageBorrows { get; } = "ManageBorrows";
    public static string ManageModels { get; } = "ManageModels";


    public static int GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(Claims.UserID);
        if (id == null)
        {
            throw new ArgumentException("User not logged in");
        }

        return int.Parse(id);
    }
}