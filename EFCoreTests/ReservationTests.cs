using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Middlewares;
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
        var curService = new CurrencyService();
        Loc = new LocalizationService(new Utils.Clock14082022(), curService);
        ReservationConstant = new ReservationConstant();
        Rates = new Rates();


        // No need to add hooks as we won't need them
        IContextHookMiddleware middleware = new ContextHookMiddleware();
        IUsageService usageService = new UsageService(Loc);
        IBorrowPersonService bpService = new BorrowPersonService(factory);
        IBorrowService BorrowService =
            new BorrowService(factory, bpService, Loc, usageService, Rates);
        ReservationsService = new ReservationService(factory, BorrowService,
            Loc, ReservationConstant, middleware);
        Factory = factory;
    }


    private IDbContextFactory<ApplicationDbContext> Factory { get; }
    private IReservationsService ReservationsService { get; }
    private ILocalizationService Loc { get; }
    private IRates Rates { get; }
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
    public async Task SuggestBorrowNoRes()
    {
        var loc = new Location()
        {
            Building = 'a',
        };
        var man = new Manual()
        {
            FileName = "jjj"
        };
        var wms = new List<WashingMachine>
        {
            new() {Location = loc, Manual = man},
            new() {Location = loc, Manual = man},
            new() {Location = loc, Manual = man, Status = Status.Broken},
        };
        await using var context = Factory.CreateDbContext();
        await context.WashingMachines.AddRangeAsync(wms);


        await context.SaveChangesAsync();
        var dur = Duration.FromHours(2);
        var suggestions =
            await ReservationsService
                .SuggestReservation<WashingMachine>(dur, 5);
        // Should be pickup up by inbetween
        Assert.Equal(2, suggestions.Length);
        var start = Loc.Now + ReservationConstant.MinDurBeforeReservation +
                    ReservationConstant.SuggestReservationDurForBorrow;
        var startLoc = start.InZone(Loc.TimeZone).LocalDateTime;
        Assert.Equal(startLoc, suggestions[0].start);
        Assert.Equal(startLoc, suggestions[1].start);
        var end = start + dur;
        var endLoc = end.InZone(Loc.TimeZone).LocalDateTime;
        Assert.Equal(suggestions[0].end, endLoc);
    }

    [Fact]
    public async Task SuggestBorrowInBetween()
    {
        var loc = new Location() {Building = 'a',};
        var man = new Manual() {FileName = "jjj"};
        var user = new ApplicationUser() {UserName = "jjj"};
        var wms = new List<WashingMachine>
        {
            new() {Location = loc, Manual = man},
        };

        var res = new List<Reservation>
        {
            new()
            {
                User = user,
                Start = Loc.Now + Duration.FromHours(3),
                End = Loc.Now + Duration.FromHours(5),
                BorrowableEntity = wms[0],
            },

            new()
            {
                User = user,
                Start = Loc.Now + Duration.FromHours(6),
                End = Loc.Now + Duration.FromHours(7),
                BorrowableEntity = wms[0],
            },
            new()
            {
                User = user,
                Start = Loc.Now + Duration.FromHours(9),
                End = Loc.Now + Duration.FromHours(10),
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
        var start = (Loc.Now + Duration.FromHours(7)).InZone(Loc.TimeZone)
            .LocalDateTime;
        var end = (Loc.Now + Duration.FromHours(9)).InZone(Loc.TimeZone)
            .LocalDateTime;
        Assert.Equal(suggestions[0].start, start);
        Assert.Equal(suggestions[0].end, end);

        await context.AddAsync(new Reservation()
        {
            User = user,
            Start = Loc.Now + Duration.FromHours(7),
            End = Loc.Now + Duration.FromHours(9),
            BorrowableEntity = wms[0],
        });
        await context.SaveChangesAsync();


        suggestions =
            await ReservationsService
                .SuggestReservation<WashingMachine>(dur, 5);

        Assert.Single(suggestions);
        start = (Loc.Now + Duration.FromHours(10)).InZone(Loc.TimeZone)
            .LocalDateTime;
        end = (Loc.Now + Duration.FromHours(12)).InZone(Loc.TimeZone)
            .LocalDateTime;
        Assert.Equal(suggestions[0].start, start);
        Assert.Equal(suggestions[0].end, end);
    }
}