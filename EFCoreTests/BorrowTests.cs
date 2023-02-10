using System;
using System.Threading.Tasks;
using App.Data;
using App.Data.Constants;
using App.Data.EFCoreServices;
using App.Data.Models;
using App.Data.ServiceInterfaces;
using App.Data.Utils;
using App.Middlewares;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace EFCoreTests;

public class _TEST_DB_BORR : DBFullFactory
{
    public _TEST_DB_BORR() : base("test_borrows")
    {
    }
}

public class BorrowTests : IClassFixture<_TEST_DB_BORR>, IDisposable
{
    public BorrowTests(_TEST_DB_BORR factory)
    {
        var middleware = new ContextHookMiddleware();
        Clock = new Utils.CustomizableClock();
        Rates = new Utils.TestRates();

        // No need to add hooks as we won't need them
        IUserService userService = new UserService(factory, Rates ,new CurrencyService());
        IUsageService usageService = new UsageService(Clock, new UsageContants());
        IBorrowPersonService bpService = new BorrowPersonService(factory);
        WmService =
            new CrudService<WashingMachine>(factory, middleware);
        BorrowService = new BorrowService(factory, bpService,
            usageService, Rates, Clock, userService);
        Factory = factory;
        BPService = new BorrowPersonService(factory);
    }

    private IDbContextFactory<ApplicationDbContext> Factory { get; }
    private Utils.CustomizableClock Clock { get; }
    private IRates Rates { get; }
    private IBorrowService BorrowService { get; }
    private IBorrowPersonService BPService { get; }
    private CrudService<WashingMachine> WmService { get; }


    public void Dispose()
    {
        using var context = Factory.CreateDbContext();
        var wms = context.WashingMachines;
        context.RemoveRange(wms);
        var res = context.Reservations;
        context.RemoveRange(res);
        var users = context.Users;
        foreach (var user in users)
        {
            user.Cash = 0;
        }
        context.SaveChanges();
    }

    [Fact]
    public async Task BorrowOfNonFreeMachine()
    {
        var bp = await BPService.GetByIdAsync(1);
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Broken,
            Manufacturer = "Test",
        };
        var entity = await WmService.CreateAsync(wm);
        var startTime = Clock.GetCurrentInstant();

        // Check if raises ArgumentException
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await BorrowService.AddBorrowAsync(new Borrow()
            {
                BorrowPerson = bp!,
                BorrowableEntity = entity,
                Start = startTime,
                End = null
            });
        });
    }
    
    [Fact]
    public async Task BorrowOfFreeMachineNoUserCash()
    {
        var bp = new BorrowPerson()
        {
            UserID = 1,
            Name = "Test",
            Surname = "Test",
        };
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Free,
            Manufacturer = "Test",
        };
        var entity = await WmService.CreateAsync(wm);
        var startTime = Clock.GetCurrentInstant();

        // Check if raises ArgumentException
        var borrowEnd = Clock.GetCurrentInstant() + Duration.FromMinutes(30);
        var borrow = await BorrowService.AddBorrowAsync(new Borrow()
        {
            BorrowPerson = bp,
            BorrowableEntity = entity,
            Start = startTime,
            End = null
        });
        Clock.SetTime(borrowEnd);
        await BorrowService.EndBorrowAsync(borrow, false);

        await using var context = await Factory.CreateDbContextAsync();
        var wmFromDb = await context.WashingMachines
            .FirstOrDefaultAsync(x => x.BorrowableEntityID == wm.BorrowableEntityID);
        
        Assert.Equal(Status.Free, wmFromDb!.Status);
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == bp.UserID);
        Assert.Equal(0, user!.Cash);
    }

    [Fact]
    public async Task BorrowOfFreeMachineUserCash()
    {
        var bp = new BorrowPerson()
        {
            UserID = 1,
            Name = "Test",
            Surname = "Test",
        };
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Free,
            Manufacturer = "Test",
        };
        var entity = await WmService.CreateAsync(wm);
        var startTime = Clock.GetCurrentInstant();

        // Check if raises ArgumentException
        var borrowEnd = Clock.GetCurrentInstant() + Duration.FromMinutes(30);
        var borrow = await BorrowService.AddBorrowAsync(new Borrow()
        {
            BorrowPerson = bp,
            BorrowableEntity = entity,
            Start = startTime,
            End = null
        });
        Clock.SetTime(borrowEnd);
        await BorrowService.EndBorrowAsync(borrow, true);

        await using var context = await Factory.CreateDbContextAsync();
        var wmFromDb = await context.WashingMachines
            .FirstOrDefaultAsync(x => x.BorrowableEntityID == wm.BorrowableEntityID);
        
        Assert.Equal(Status.Free, wmFromDb!.Status);
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == bp.UserID);
        Assert.Equal(-(Rates.FlatBorrowPrice + Rates.PricePerHalfHour), user!.Cash);
    }

    [Fact]
    public async Task Price()
    {
        var bp = await BPService.GetByIdAsync(1);
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Free,
            Manufacturer = "Test",
        };
        var entity = await WmService.CreateAsync(wm);
        var startTime = Clock.GetCurrentInstant();
        var end = startTime + Duration.FromHours(1);
        var borrow = await BorrowService.AddBorrowAsync(new Borrow()
        {
            BorrowPerson = bp!,
            BorrowableEntity = entity,
            Start = startTime,
            End = end
        });
        var price = await BorrowService.GetPriceAsync(borrow);
        var expected = Rates.FlatBorrowPrice + Rates.PricePerHalfHour * 2;
        Assert.Equal(expected, price.Amount);
    }
}