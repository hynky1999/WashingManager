using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Auth.Models;
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

public class _TEST_DB_RESS : DBEmpty
{
    public _TEST_DB_RESS() : base("test_reservation")
    {
    }
}

public class ReservationTests : IClassFixture<_TEST_DB_RESS>, IDisposable
{
    public ReservationTests(_TEST_DB_RESS factory)
    {
        Clock = new Utils.Clock14082022();
        ReservationConstant = new ReservationConstant();
        Rates = new Rates();


        // No need to add hooks as we won't need them
        IContextHookMiddleware middleware = new ContextHookMiddleware();
        IUsageService usageService = new UsageService(Clock, new UsageContants());
        IBorrowPersonService bpService = new BorrowPersonService(factory);
        IUserService userService = new UserService(factory, Rates, new CurrencyService());
        IBorrowService BorrowService =
            new BorrowService(factory, bpService, usageService, Rates, Clock, userService);
        ReservationsService = new ReservationService(factory, BorrowService,
            ReservationConstant, middleware, Clock);
        Factory = factory;
    }


    private IDbContextFactory<ApplicationDbContext> Factory { get; }
    private IReservationsService ReservationsService { get; }
    private IRates Rates { get; }
    private IClock Clock { get; }
    private IReservationConstant ReservationConstant { get; }


    public void Dispose()
    {
        using var context = Factory.CreateDbContext();
        var wms = context.WashingMachines;
        context.RemoveRange(wms);
        var res = context.Reservations;
        context.RemoveRange(res);
        context.SaveChanges();
    }

    [Fact]
    public async Task SuggestBorrowAtStartWithRes()
    {
        var user = new ApplicationUser() {UserName = "jjj"};
        var wms = new List<WashingMachine>
        {
            new(),
        };
        var res = new List<Reservation>
        {
            new()
            {
                User = user,
                Start = Clock.GetCurrentInstant()+ Duration.FromHours(3),
                End = Clock.GetCurrentInstant()+ Duration.FromHours(5),
                BorrowableEntity = wms[0],
            }
        };
        await using var context = Factory.CreateDbContext();
        await context.WashingMachines.AddRangeAsync(wms);
        await context.Reservations.AddRangeAsync(res);
        await context.SaveChangesAsync();

        var dur = Duration.FromHours(2);
        var suggestions =
            await ReservationsService
                .SuggestReservation<WashingMachine>(dur, 5);
        // Should be pickup up by minStart
        Assert.Single(suggestions);
        var start = Clock.GetCurrentInstant()+ ReservationConstant.MinDurBeforeReservation +
                    ReservationConstant.SuggestReservationDurForBorrow;
        Assert.Equal(start, suggestions[0].start);
        var end = start + dur;
        Assert.Equal(end, suggestions[0].end);
    }

    [Fact]
    public async Task SuggestBorrowNoRes()
    {
        var wms = new List<WashingMachine>
        {
            new(),
            new(),
            new() {Status = Status.Broken},
        };
        await using var context = Factory.CreateDbContext();
        await context.WashingMachines.AddRangeAsync(wms);


        await context.SaveChangesAsync();
        var dur = Duration.FromHours(2);
        var suggestions =
            await ReservationsService
                .SuggestReservation<WashingMachine>(dur, 5);
        // Should be pickup up by minStart
        Assert.Equal(2, suggestions.Length);
        var start =Clock.GetCurrentInstant() + ReservationConstant.MinDurBeforeReservation +
                    ReservationConstant.SuggestReservationDurForBorrow;
        Assert.Equal(start, suggestions[0].start);
        Assert.Equal(start, suggestions[1].start);
        var end = start + dur;
        Assert.Equal(end, suggestions[0].end);
    }

    [Fact]
    public async Task SuggestBorrowInBetween()
    {
        var user = new ApplicationUser() {UserName = "jjj"};
        var wms = new List<WashingMachine>
        {
            new(),
        };

        var res = new List<Reservation>
        {
            new()
            {
                User = user,
                Start = Clock.GetCurrentInstant(),
                End = Clock.GetCurrentInstant() + Duration.FromHours(3),
                BorrowableEntity = wms[0],
            },

            new()
            {
                User = user,
                Start = Clock.GetCurrentInstant() + Duration.FromHours(3),
                End = Clock.GetCurrentInstant() + Duration.FromHours(5),
                BorrowableEntity = wms[0],
            },

            new()
            {
                User = user,
                Start = Clock.GetCurrentInstant() + Duration.FromHours(6),
                End = Clock.GetCurrentInstant() + Duration.FromHours(7),
                BorrowableEntity = wms[0],
            },
            new()
            {
                User = user,
                Start = Clock.GetCurrentInstant() + Duration.FromHours(9),
                End = Clock.GetCurrentInstant() + Duration.FromHours(10),
                BorrowableEntity = wms[0],
            },
        };
        using var context = Factory.CreateDbContext();
        context.WashingMachines.AddRange(wms);
        context.Reservations.AddRange(res);
        context.SaveChanges();

        var dur = Duration.FromHours(2);
        var suggestions =
            await ReservationsService
                .SuggestReservation<WashingMachine>(dur, 5);

        Assert.Single(suggestions);
        var start = Clock.GetCurrentInstant() + Duration.FromHours(7);
        var end = Clock.GetCurrentInstant() + Duration.FromHours(9);
        Assert.Equal(suggestions[0].start, start);
        Assert.Equal(suggestions[0].end, end);

        await context.AddAsync(new Reservation()
        {
            User = user,
            Start = Clock.GetCurrentInstant() + Duration.FromHours(7),
            End = Clock.GetCurrentInstant() + Duration.FromHours(9),
            BorrowableEntity = wms[0],
        });
        await context.SaveChangesAsync();


        suggestions =
            await ReservationsService
                .SuggestReservation<WashingMachine>(dur, 5);

        Assert.Single(suggestions);
        start = Clock.GetCurrentInstant() + Duration.FromHours(10);
        end = Clock.GetCurrentInstant() + Duration.FromHours(12);
        Assert.Equal(suggestions[0].start, start);
        Assert.Equal(suggestions[0].end, end);
    }
}