using System.Security.Claims;
using AntDesign;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime.Extensions;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data;
using PrackyASusarny.Data.Models;

// ReSharper disable CoVariantArrayConversion

namespace PrackyASusarny.Utils;
// Scafolded from https://learn.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?source=recommendations&view=aspnetcore-6.0

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider,
        string testUserPw)
    {
        // For sample purposes seed both with the same password.
        // Password is set with the following:
        // dotnet user-secrets set SeedUserPW <pw>
        // The admin user can do anything

        var admin = await EnsureUser(serviceProvider, testUserPw,
            "kydlicek.hynek@gmail.com");
        await EnsureRole(serviceProvider, IdentityRoles.Administrator, new[]
        {
            new Claim(Claims.ManageBorrows, true.ToString()),
            new Claim(Claims.ManageModels, true.ToString()),
            new Claim(Claims.ManageUsers, true.ToString())
        });
        await AddToRole(serviceProvider, admin.Id, IdentityRoles.Administrator);


        // allowed user can create and edit contacts that they create
        var manager = await EnsureUser(serviceProvider, testUserPw,
            "manager@contoso.com");
        await EnsureRole(serviceProvider, IdentityRoles.Receptionist, new[]
        {
            new Claim("ManageBorrows", true.ToString())
        });
        await AddToRole(serviceProvider, manager.Id,
            IdentityRoles.Receptionist);
        var factory = serviceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        SeedDb(factory, new ApplicationUser[] {admin, manager});
    }


    private static async Task<ApplicationUser> EnsureUser(
        IServiceProvider serviceProvider,
        string testUserPw, string userName)
    {
        var userManager =
            serviceProvider.GetService<UserManager<ApplicationUser>>();

        var user = await userManager!.FindByNameAsync(userName);
        if (user != null)
        {
            return user;
        }

        user = new ApplicationUser
        {
            Name = "Hyne",
            Surname = "Kydlicek",
            EmailConfirmed = true,
            UserName = userName,
        };
        var result = await userManager.CreateAsync(user, testUserPw);


        if (user == null)
            throw new Exception("The password is probably not strong enough!");


        await userManager.AddClaimAsync(user,
            new Claim(Claims.UserID, user.Id.ToString()));


        return user;
    }

    private static async Task AddToRole(IServiceProvider serviceProvider,
        int userId, string role)
    {
        var userManager =
            serviceProvider.GetService<UserManager<ApplicationUser>>();
        var user = await userManager!.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new Exception("The password is probably not strong enough!");

        await userManager.AddToRoleAsync(user, role);
    }

    private static async Task EnsureRole(IServiceProvider serviceProvider,
        string role, IEnumerable<Claim> claims)
    {
        var roleManager = serviceProvider.GetService<RoleManager<Role>>();

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
        Borrow[] borrows = Enumerable.Range(0, 30)
            .Select(_ => CreateRandomBorrow(persons, wm))
            .Where(x => x is not null).ToArray()!;

        List<Reservation> reservations = new();
        Enumerable.Range(0, 30).ForEach(_ =>
        {
            var r = CreateRandomReservation(users, wm);
            // If not overlap with previous reservations
            if (reservations.Where(resOld =>
                    resOld.BorrowableEntityID == r.BorrowableEntityID &&
                    (resOld.Start < r.End && resOld.End > r.Start)).Count() ==
                0)
            {
                reservations.Add(r);
            }
        });

        context.AttachRange(users);
        context.AddRange(wmUsage);
        context.AddRange(manuals);
        context.AddRange(locs);
        context.AddRange(wm);
        context.AddRange(persons);
        context.AddRange(borrows);
        context.AddRange(reservations);
        context.SaveChanges();
    }

    private static IEnumerable<BorrowableEntityUsage<WashingMachine>>
        CreateWmUsage()
    {
        return Enum.GetValues<IsoDayOfWeek>().Select(d =>
        {
            return new BorrowableEntityUsage<WashingMachine>
            {
                DayId = d
            };
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

    private static Borrow? CreateRandomBorrow(BorrowPerson[] persons,
        WashingMachine[] wm)
    {
        var rnd = new Random();
        var availableWms = wm.Where(x => x.Status == Status.Free).ToArray();
        if (availableWms.Length == 0) return null;

        var start = DateTime.UtcNow.AddDays(-rnd.Next(1, 100)).ToInstant();
        Instant? end = rnd.Next(0, 2) == 0
            ? DateTime.UtcNow.AddDays(rnd.Next(2, 100)).ToInstant()
            : null;
        var chosenWm = availableWms[rnd.Next(availableWms.Length)];
        if (end != null) chosenWm.Status = Status.Taken;

        return new Borrow
        {
            BorrowPerson = persons[rnd.Next(persons.Length)],
            BorrowableEntity = chosenWm,
            startDate = start,
            endDate = end
        };
    }

    private static Reservation CreateRandomReservation(ApplicationUser[] users,
        WashingMachine[] wm)
    {
        var rnd = new Random();
        var secsWeek = 60 * 60 * 24 * 7;
        var start = DateTime.UtcNow.AddSeconds(rnd.Next(1, secsWeek));
        var end = start.AddSeconds(rnd.Next(1, secsWeek));
        return new Reservation
        {
            User = users[rnd.Next(users.Length)],
            Start = start.ToInstant(),
            BorrowableEntity = wm[rnd.Next(wm.Length)],
            End = end.ToInstant(),
        };
    }
}