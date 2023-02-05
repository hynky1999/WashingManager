using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moq;
using NodaTime;
using PrackyASusarny.Auth.Utils;
using PrackyASusarny.Data;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.LocServices;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Middlewares;
using PrackyASusarny.ServerServices;
using PrackyASusarny.Utils;
using Xunit;

namespace EFCoreTests;

public class ResConstTest : IReservationConstant
{
    public Duration MaxReservationDur => Duration.FromHours(10);
    public Duration MinReservationDur => Duration.FromMinutes(0);
    public Duration MinDurBeforeReservation => Duration.FromMinutes(0);
    public Duration MinReservationCancelDur => Duration.FromMinutes(0);
    public Duration ReservationPostponeDur => Duration.FromMinutes(30);
    public Duration SuggestReservationDurForBorrow => Duration.FromMinutes(30);
    public int MaxReservationsAtTime => 5;
}

public class RatesTest : IRates
{
    public int WMpricePerHalfHour => 10;
    public int FlatBorrowPrice => 100;
    public int WMpricePerOverRes => 20;
    public int WMNoBorrowPenalty => 30;
    public Currency DBCurrency => Currency.CZK;
}

public class _TEST_DB_RESMAN : DBFullFactory
{
    public _TEST_DB_RESMAN() : base("test_resman")
    {
    }
}

public class ReservationManagerTests : IClassFixture<_TEST_DB_RESMAN>,
    IDisposable
{
    private Utils.CustomizableClock _clock;

    public ReservationManagerTests(_TEST_DB_RESMAN factory)
    {
        CurrencyService = new CurrencyService();
        _clock = new Utils.CustomizableClock();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json")
            .Build();
        Loc = new LocalizationService(_clock, CurrencyService,
            Mock.Of<IStringLocalizer<LocalizationService>>(), configuration);
        IContextHookMiddleware middleware = new ContextHookMiddleware();
        IUsageService usageService = new UsageService(Loc, new UsageContants());
        ICurrencyService currencyService = new CurrencyService();
        BPService = new BorrowPersonService(factory);
        ReservationConstant = new ResConstTest();
        Rates = new RatesTest();
        IUserService userService =
            new UserService(factory, Rates, currencyService);
        IReservationConstant reservationConstant = new ResConstTest();
        WMService =
            new CrudService<WashingMachine>(factory, middleware);
        BorrowService =
            new BorrowService(factory, BPService, Loc, usageService, Rates);
        ReservationService = new ReservationService(factory,
            BorrowService, Loc, reservationConstant, middleware);


        ReservationManager = new ReservationManager(userService, Loc,
            BorrowService, reservationConstant, Rates, middleware, factory);


        ProgramInit.InitializeHooks(middleware, ReservationManager);
        Factory = factory;
    }

    private CurrencyService CurrencyService { get; }
    private IDbContextFactory<ApplicationDbContext> Factory { get; }
    private IReservationsService ReservationService { get; }
    private IReservationManager ReservationManager { get; }
    private ILocalizationService Loc { get; }
    private IRates Rates { get; }
    private IReservationConstant ReservationConstant { get; }
    private ICrudService<WashingMachine> WMService { get; }
    private IBorrowService BorrowService { get; }
    private IBorrowPersonService BPService { get; }


    public void Dispose()
    {
        using var context = Factory.CreateDbContext();
        var res = context.Reservations;
        context.RemoveRange(res);
        var borrows = context.Borrows;
        context.RemoveRange(borrows);
        var users = context.Users;
        var wm = context.WashingMachines.Find(10);
        if (wm != null)
        {
            context.Remove(wm);
        }

        foreach (var user in users)
        {
            user.Cash = 0;
        }

        context.SaveChanges();
    }

    [Fact]
    public async Task CreateAndRemoveQueueNonExistent()
    {
        // Non existing BE should pass
        await ReservationManager.Create(-1);
        ReservationManager.Remove(-1);
    }

    [Fact]
    public async Task AutoCreateQueue()
    {
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Free,
            Manufacturer = "Test",
        };
        await WMService.CreateAsync(wm);
        Assert.Equal(1, ReservationManager.Count);
        await WMService.DeleteAsync(wm);
        Assert.Equal(0, ReservationManager.Count);
    }


    [Fact]
    public async Task NoPickup()
    {
        int userID = 1;
        var userPrincipal = Mock.Of<ClaimsPrincipal>(x =>
            x.FindFirst(Claims.UserID) ==
            new Claim(Claims.UserID, userID.ToString()));


        var clockTime = Instant.FromUtc(2021, 1, 1, 0, 0);
        _clock.SetTime(clockTime);
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Free,
            Manufacturer = "Test",
        };
        await WMService.CreateAsync(wm);

        var start = clockTime + Duration.FromSeconds(1);
        var end = start + Duration.FromSeconds(5);
        await ReservationService.CreateReservationAsync(
            start.InZone(Loc.TimeZone).LocalDateTime,
            end.InZone(Loc.TimeZone).LocalDateTime, userPrincipal, wm);

        // Since we modify time manually it can happen that manager is notified after the clock is set this ensure that it is not the case
        _clock.SetTime(end - Duration.FromSeconds(1));
        await Task.Delay(1000);
        _clock.SetTime(end);
        await Task.Delay(7000);

        using var ctx = Factory.CreateDbContext();
        var user = await ctx.Users.FindAsync(userID);
        // Penalty applied for not picking up
        Assert.Equal(-Rates.WMNoBorrowPenalty, user!.Cash);
    }

    [Fact]
    public async Task Pickup()
    {
        int userID = 1;
        var userPrincipal = Mock.Of<ClaimsPrincipal>(x =>
            x.FindFirst(Claims.UserID) ==
            new Claim(Claims.UserID, userID.ToString()));


        var clockTime = Instant.FromUtc(2021, 1, 1, 0, 0);
        _clock.SetTime(clockTime);
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Free,
            Manufacturer = "Test",
        };
        await WMService.CreateAsync(wm);

        var start = clockTime + Duration.FromSeconds(1);
        var end = start + Duration.FromSeconds(5);
        var res = await ReservationService.CreateReservationAsync(
            start.InZone(Loc.TimeZone).LocalDateTime,
            end.InZone(Loc.TimeZone).LocalDateTime, userPrincipal, wm);

        // Since we modify time manually it can happen that manager is notified after the clock is set this ensure that it is not the case
        _clock.SetTime(end - Duration.FromSeconds(1));
        await Task.Delay(3000);
        _clock.SetTime(end);
        var bp = await BPService.GetByIdAsync(1);
        var borrow = new Borrow()
        {
            BorrowableEntity = wm,
            BorrowPerson = bp!,
            Start = start,
            End = end,
            ReservationID = res!.ReservationID
        };
        await BorrowService.AddBorrowAsync(borrow);
        await Task.Delay(7000);


        using var ctx = Factory.CreateDbContext();
        var user = await ctx.Users.FindAsync(userID);
        // Penalty applied for not picking up
        Assert.Equal(0, user!.Cash);
    }

    [Fact]
    public async Task OverBorrow()
    {
        int userID = 1;
        var userPrincipal = Mock.Of<ClaimsPrincipal>(x =>
            x.FindFirst(Claims.UserID) ==
            new Claim(Claims.UserID, userID.ToString()));


        var clockTime = Instant.FromUtc(2021, 1, 1, 0, 0);
        _clock.SetTime(clockTime);
        var wm = new WashingMachine()
        {
            BorrowableEntityID = 10,
            LocationID = 1,
            ManualID = 1,
            Status = Status.Free,
            Manufacturer = "Test",
        };
        await WMService.CreateAsync(wm);

        var start = clockTime + Duration.FromSeconds(1);
        var end = start + Duration.FromSeconds(5);
        var start2 = end;
        var end2 = start2 + Duration.FromSeconds(5);

        var res2 = await ReservationService.CreateReservationAsync(
            start2.InZone(Loc.TimeZone).LocalDateTime,
            end2.InZone(Loc.TimeZone).LocalDateTime, userPrincipal, wm);

        // Should override the timer of res2
        var res1 = await ReservationService.CreateReservationAsync(
            start.InZone(Loc.TimeZone).LocalDateTime,
            end.InZone(Loc.TimeZone).LocalDateTime, userPrincipal, wm);

        var bp = await BPService.GetByIdAsync(1);
        var borrow = new Borrow()
        {
            BorrowableEntity = wm,
            BorrowPerson = bp!,
            Start = start,
            End = null,
            ReservationID = res1!.ReservationID
        };

        // Since we modify time manually it can happen that manager is notified after the clock is set this ensure that it is not the case
        _clock.SetTime(end - Duration.FromSeconds(2));
        await Task.Delay(2000);
        _clock.SetTime(end);
        await BorrowService.AddBorrowAsync(borrow);
        await Task.Delay(8000);

        using var ctx = await Factory.CreateDbContextAsync();
        // User penalty for overborrowing
        var user = await ctx.Users.FindAsync(userID);
        Assert.Equal(-Rates.WMpricePerOverRes, user!.Cash);


        // Res1 end Rescheduled
        var res1updated = await ctx.Reservations.FindAsync(res1.ReservationID);
        Assert.NotNull(res1updated);
        Assert.Equal(res1.Start, res1updated!.Start);
        Assert.Equal(res1.End + ReservationConstant.ReservationPostponeDur,
            res1updated.End);

        // Res2 rescheduled
        var res2updated = await ctx.Reservations.FindAsync(res2!.ReservationID);
        Assert.NotNull(res2updated);
        Assert.Equal(res1updated.End, res2updated!.Start);
        Assert.Equal(res2updated.Start + Duration.FromSeconds(5),
            res2updated.End);
    }
}