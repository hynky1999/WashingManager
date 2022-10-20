using Microsoft.AspNetCore.Identity;
using PrackyASusarny.Identity;

namespace PrackyASusarny;
// Scafolded from https://learn.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?source=recommendations&view=aspnetcore-6.0

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
    {
        // For sample purposes seed both with the same password.
        // Password is set with the following:
        // dotnet user-secrets set SeedUserPW <pw>
        // The admin user can do anything

        var adminId = await EnsureUser(serviceProvider, testUserPw, "kydlicek.hynek@gmail.com");
        await EnsureRole(serviceProvider, adminId, IdentityRoles.Administrator);

        // allowed user can create and edit contacts that they create
        var managerId = await EnsureUser(serviceProvider, testUserPw, "manager@contoso.com");
        await EnsureRole(serviceProvider, managerId, IdentityRoles.Receptionist);

        SeedDb(adminId);
    }


    private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
        string testUserPw, string userName)
    {
        var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

        var user = await userManager!.FindByNameAsync(userName);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = userName,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, testUserPw);
        }

        if (user == null)
        {
            throw new Exception("The password is probably not strong enough!");
        }

        return user.Id;
    }

    private static async Task EnsureRole(IServiceProvider serviceProvider,
        string uid, string role)
    {
        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

        if (roleManager == null)
        {
            throw new Exception("roleManager null");
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
        var user = await userManager!.FindByIdAsync(uid);

        if (user == null)
        {
            throw new Exception("The testUserPw password was probably not strong enough!");
        }

        await userManager.AddToRoleAsync(user, role);
    }

    public static void SeedDb(string adminId)
    {
    }
}