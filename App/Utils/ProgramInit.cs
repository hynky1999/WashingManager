using System.Security.Claims;
using App.Auth.Models;
using App.Auth.Utils;
using App.Data;
using App.Data.Models;
using App.Data.ServiceInterfaces;
using App.Data.Utils;
using App.Middlewares;
using App.ServerServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// ReSharper disable CoVariantArrayConversion
namespace App.Utils;

/// <summary>
/// This class consists of methods used for program initialization.
/// </summary>
public static class ProgramInit
{
    /// <summary>
    /// Initializes hooks to <see cref="IContextHookMiddleware"/>
    /// The only hooks added are for BE deletion/creation -> queue creations/deletion
    /// Reservation deletion/modification/creation -> queue on change
    /// </summary>
    /// <param name="middleware"></param>
    /// <param name="reservationManager"></param>
    public static void InitializeHooks(IContextHookMiddleware middleware,
        IReservationManager reservationManager)
    {
        // Whatever happens to Reservation inform reservation manager
        foreach (var state in new[]
                 {
                     EntityState.Added, EntityState.Modified,
                     EntityState.Deleted
                 })
        {
            middleware.AddContextHook<Reservation>(state,
                async (entity) =>
                {
                    await reservationManager.OnChange(entity
                        .BorrowableEntityID);
                });
        }

        // BorrowableEntity added create queue
        var beCreateHook = async (BorrowableEntity entity) =>
        {
            await reservationManager.Create(entity.BorrowableEntityID);
        };
        var beRemoveHook = (BorrowableEntity entity) =>
        {
            reservationManager.Remove(entity.BorrowableEntityID);
            return Task.CompletedTask;
        };
        var geAddHook =
            typeof(IContextHookMiddleware).GetMethod(
                nameof(IContextHookMiddleware.AddContextHook))!;
        foreach (var type in new[]
                 {
                     typeof(BorrowableEntity), typeof(WashingMachine),
                     typeof(DryingRoom)
                 })
        {
            // Add and Remove hooks
            geAddHook.MakeGenericMethod(type).Invoke(middleware,
                new object[] {EntityState.Added, beCreateHook});
            geAddHook.MakeGenericMethod(type).Invoke(middleware,
                new object[] {EntityState.Deleted, beRemoveHook});
            // No need for modify hook as there is no way to modify ID of BE
        }
    }

    /// <summary>
    /// Initializes queues of <see cref="IReservationManager"/> based on BEs in database
    /// </summary>
    /// <param name="reservationManager"></param>
    /// <param name="contextFactory"></param>
    /// <param name="beService"></param>
    public static async Task InitializeQueues(
        IReservationManager reservationManager,
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IBorrowableEntityService beService)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var bes = await beService
            .GetAllBorrowableEntitites<BorrowableEntity>();

        foreach (var be in bes)
        {
            await reservationManager.Create(be.BorrowableEntityID);
        }
    }

    /// <summary>
    /// Initialize the database with default user accounts and roles.
    /// </summary>
    /// <param name="dbContextFactory"></param>
    /// <param name="userManager"></param>
    /// <param name="roleManager"></param>
    /// <param name="admin">Admin User</param>
    /// <param name="adminPass">Admin Password</param>
    /// <param name="manager">Manager User</param>
    /// <param name="managerPass">Manager Password</param>
    /// <param name="initData">Should db be initialized with random data ?</param>
    public static async Task InitializeDB(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        UserManager<ApplicationUser> userManager, RoleManager<Role> roleManager,
        ApplicationUser? admin, string adminPass, ApplicationUser? manager,
        string managerPass, bool initData = true)
    {
        // Roles setting
        await EnsureRole(roleManager, IdentityRoles.Administrator, new[]
        {
            new Claim(Claims.ManageBorrows, true.ToString()),
            new Claim(Claims.ManageModels, true.ToString()),
            new Claim(Claims.ManageUsers, true.ToString())
        });
        await EnsureRole(roleManager, IdentityRoles.Receptionist, new[]
        {
            new Claim(Claims.ManageBorrows, true.ToString()),
        });


        if (admin != null)
        {
            var user = await EnsureUser(userManager, admin, adminPass);
            await AddToRole(userManager, user.Id,
                IdentityRoles.Administrator);
        }

        if (manager != null)
        {
            var user = await EnsureUser(userManager, manager, managerPass
            );
            await AddToRole(userManager, user.Id,
                IdentityRoles.Receptionist);
        }

        if (initData && admin != null && manager != null)
        {
            SeedDb(dbContextFactory, new[] {admin, manager});
        }
    }


    private static async Task<ApplicationUser> EnsureUser(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user, string password)
    {
        var dupUser = await userManager.FindByNameAsync(user.UserName);
        if (dupUser != null)
        {
            return dupUser;
        }

        IdentityResult result =
            await userManager.CreateWithClaimAsync(user, password);


        if (!result.Succeeded)
            throw new Exception("The password is probably not strong enough!");
        return user;
    }

    private static async Task AddToRole(
        UserManager<ApplicationUser> userManager,
        int userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new Exception("User not found");

        await userManager.AddToRoleAsync(user, role);
    }

    private static async Task EnsureRole(RoleManager<Role> roleManager,
        string role, IEnumerable<Claim> claims)
    {
        if (roleManager == null) throw new Exception("roleManager null");

        var newRole = new Role(role);
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(newRole);
            foreach (var claim in claims)
                await roleManager.AddClaimAsync(newRole, claim);
        }
    }

    private static void SeedDb(IDbContextFactory<ApplicationDbContext> factory,
        ApplicationUser[] users)
    {
        using var context = factory.CreateDbContext();
        if (context.WashingMachines.Any()) return;

        var wmUsage = CreateWmUsage();
        var manuals = Enumerable.Range(0, 10).Select(_ => CreateRandomManual())
            .ToArray();
        var locs = Enumerable.Range(0, 20).Select(_ => CreateRandomLocation())
            .ToArray();
        var wm = Enumerable.Range(0, 20)
            .Select(_ => CreateRandomWashingMachines(locs, manuals)).ToArray();

        var persons = Enumerable.Range(0, 10)
            .Select(_ => CreateRandomBorrowPerson()).ToArray();

        context.AttachRange(users);
        context.AddRange(wmUsage);
        context.AddRange(manuals);
        context.AddRange(locs);
        context.AddRange(wm);
        context.AddRange(persons);
        context.SaveChanges();
    }

    private static IEnumerable<BorrowableEntityUsage<WashingMachine>>
        CreateWmUsage()
    {
        return Enum.GetValues<IsoDayOfWeek>().Select(d =>
            new BorrowableEntityUsage<WashingMachine>
            {
                DayId = d
            });
    }

    private static string RandomString(int length)
    {
        var rnd = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rnd.Next(s.Length)]).ToArray());
    }

    private static BorrowPerson CreateRandomBorrowPerson()
    {
        return new BorrowPerson
        {
            Name = RandomString(10),
            Surname = RandomString(10)
        };
    }

    private static Manual CreateRandomManual()
    {
        var rnd = new Random().Next(20);
        return new Manual
        {
            FileName = "/manuals/" + rnd + ".pdf",
        };
    }

    private static WashingMachine CreateRandomWashingMachines(Location[] locs,
        Manual[] mans)
    {
        var rnd = new Random();
        var manfacs = new[]
        {
            "Bosh", "Samsung", "LG", "Miele", "AEG", "Electrolux", "Whirlpool",
            "Siemens", "Zanussi", "Hotpoint"
        };
        var stats = Enum.GetValues<Status>().Where(x => x != Status.Taken)
            .ToArray();
        return new WashingMachine
        {
            Location = locs[rnd.Next(locs.Length)],
            Manufacturer = manfacs[rnd.Next(manfacs.Length)],
            Manual = mans[rnd.Next(mans.Length)],
            Status = stats[rnd.Next(stats.Length)]
        };
    }

    private static Location CreateRandomLocation()
    {
        var rnd = new Random();
        var avbBuildings = new[] {'A', 'B'};
        var maxFloor = 20;
        var maxRoom = 20;
        var maxDoorNum = 3;
        return new Location
        {
            Floor = rnd.Next(maxFloor),
            Building = avbBuildings[rnd.Next(2)],
            RoomNum = rnd.Next(maxRoom),
            DoorNum = rnd.Next(maxDoorNum)
        };
    }
}