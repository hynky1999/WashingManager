using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using Xunit;

namespace EFCoreTests;

public class ReservationTests : IClassFixture<DBEmpty>, IDisposable
{
    public ReservationTests(DBEmpty factory)
    {
        Loc = new LocalizationService(new Clock14082022());
        var loggerFactory = new NullLoggerFactory();

        ReservationsService = new ReservationService(factory, Loc);
        Factory = factory;
    }

    private IDbContextFactory<ApplicationDbContext> Factory { get; }
    private IReservationsService ReservationsService { get; }
    private ILocalizationService Loc { get; }


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
        var start = Loc.Now + ReservationConstant.MinHoursBeforeReservation;
        var startLoc = start.InZone(Loc.TimeZone).LocalDateTime;
        Assert.Equal(suggestions[0].start, startLoc);
        Assert.Equal(suggestions[1].start, startLoc);
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