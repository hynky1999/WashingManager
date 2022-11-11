using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using PrackyASusarny.Data;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.Models;
using Xunit;

namespace EFCoreTests;

public class DbFactory : IDbContextFactory<ApplicationDbContext>
{
    private const string ConnectionString =
        @"Host=localhost;Database=efcoretest;Username=hynky;Password=sirecek007";

    private static readonly object Lock = new();
    private static bool _databaseInitialized;

    public DbFactory()
    {
        lock (Lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateDbContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    var wms = CreateWashingMachines();
                    var borrows = CreateBorrows(wms);
                    context.AddRange(wms);
                    context.AddRange(borrows);

                    var usage = CreateUsages();
                    context.AddRange(usage);
                    context.SaveChanges();
                }

                _databaseInitialized = true;
            }
        }
    }

    public Task<ApplicationDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(CreateDbContext());
    }

    public ApplicationDbContext CreateDbContext()
    {
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString, o => o.UseNodaTime()).Options);
    }

    public List<BorrowableEntityUsage<WashingMachine>> CreateUsages()
    {
        var usages = Enum.GetValues<IsoDayOfWeek>().Select(x =>
            new BorrowableEntityUsage<WashingMachine>
            {
                DayId = x
            }).ToList();
        usages[(int) IsoDayOfWeek.Tuesday].Hour5Total = 100;
        usages[(int) IsoDayOfWeek.Tuesday].Hour9Total = 800;
        usages[(int) IsoDayOfWeek.Saturday].Hour0Total = 100;
        usages[(int) IsoDayOfWeek.Saturday].Hour9Total = 900;
        usages[(int) IsoDayOfWeek.Saturday].Hour10Total = 200;
        return usages;
    }

    private List<WashingMachine> CreateWashingMachines()
    {
        var washingMachines = new List<WashingMachine>
        {
            new()
            {
                Location = new Location
                {
                    Building = 'A',
                    DoorNum = 1,
                    Floor = 0
                },
                Manual = new Manual
                {
                    FileName = "xd.pdf", Name = "xd"
                }
            }
        };
        return washingMachines;
    }

    private List<Borrow> CreateBorrows(List<WashingMachine> washingMachines)
    {
        var dater = new LocalizationService(SystemClock.Instance);
        var borrowPersons = new List<BorrowPerson>
        {
            new() {Name = "Hlynka", Surname = "Sirecek"},
            new() {Name = "Pechoun", Surname = "Buh"},
            new() {Name = "Samuel", Surname = "Lehoun"}
        };
        var borrows = new List<Borrow>
        {
            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 1, 1, 0)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            },
            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 2, 0, 0)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            },
            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 2, 5, 0)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            },
            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 2, 5, 3)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            },

            new()
            {
                BorrowableEntity = washingMachines[0],
                startDate = new LocalDateTime(2020, 1, 4, 10, 0)
                    .InZoneLeniently(dater.TimeZone).ToInstant(),
                BorrowPerson = borrowPersons[0],
                endDate = null, xmin = 0
            }
        };
        return borrows;
    }
}

public class Clock14082022 : IClock
{
    public Instant GetCurrentInstant()
    {
        return new ZonedDateTime(new LocalDateTime(2022, 8, 14, 0, 0),
            DateTimeZone.Utc, Offset.Zero).ToInstant();
    }
}

public class ChartingTests : IClassFixture<DbFactory>
{
    private readonly double _mondaysSinceStart;

    public ChartingTests(DbFactory factory)
    {
        var localization = new LocalizationService(new Clock14082022());
        var loggerFactory = new NullLoggerFactory();
        UsageService =
            new UsageChartingService<WashingMachine>(factory, localization);
        UsageUpdateService = new UsageService(localization);
        Factory = factory;

        _mondaysSinceStart = 6 / 7.0;
    }

    private UsageChartingService<WashingMachine> UsageService { get; }
    private DbFactory Factory { get; }
    private UsageService UsageUpdateService { get; }


    [Fact]
    public async Task BorrowsByDayTest()
    {
        var result =
            await UsageService.GetBorrowsByDayAsync(new LocalDate(2020, 1, 1),
                new LocalDate(2020, 1, 4));
        var expected = new[]
        {
            (new LocalDate(2020, 1, 1), 1),
            (new LocalDate(2020, 1, 2), 3),
            (new LocalDate(2020, 1, 3), 0),
            (new LocalDate(2020, 1, 4), 1)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task BorrowsByHourTest()
    {
        var date = new LocalDate(2020, 1, 2);
        var result = await UsageService.GetBorrowsByHourAsync(date);
        var expected = Enumerable.Range(0, 24)
            .Select(hour => (new LocalTime(hour, 0), 0)).ToArray();
        expected[0].Item2 = 1;
        expected[5].Item2 = 2;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task WeekUsageDayTest()
    {
        var result = await UsageService.GetWeekUsageAsync();
        var expected = new[]
        {
            (IsoDayOfWeek.Monday, 0),
            (IsoDayOfWeek.Tuesday, 900 / _mondaysSinceStart),
            (IsoDayOfWeek.Wednesday, 0),
            (IsoDayOfWeek.Thursday, 0),
            (IsoDayOfWeek.Friday, 0),
            (IsoDayOfWeek.Saturday, 1200 / _mondaysSinceStart),
            (IsoDayOfWeek.Sunday, 0)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task HourlyUsageTest()
    {
        var result =
            await UsageService.GetHourlyUsageAsync(IsoDayOfWeek.Saturday);
        var expected = Enumerable.Range(0, 24)
            .Select(x => (new LocalTime(x, 0), 0.0)).ToArray();
        expected[0].Item2 = 100 / _mondaysSinceStart;
        expected[9].Item2 = 900 / _mondaysSinceStart;
        expected[10].Item2 = 200 / _mondaysSinceStart;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task AvgUsageTest()
    {
        var result = await UsageService.GetAvgHourlyUsageAsync();
        var expected = Enumerable.Range(0, 24)
            .Select(x => (new LocalTime(x, 0), 0.0)).ToArray();
        expected[0].Item2 = 100 / _mondaysSinceStart;
        expected[5].Item2 = 100 / _mondaysSinceStart;
        expected[9].Item2 = 1700 / _mondaysSinceStart;
        expected[10].Item2 = 200 / _mondaysSinceStart;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task AddToUsageOneDayTest()
    {
        var borrow = new Borrow();
        borrow.startDate =
            new LocalDateTime(2022, 8, 13, 23, 0).InUtc().ToInstant();
        var expected = new BorrowableEntityUsage<WashingMachine>
        {
            DayId = IsoDayOfWeek.Sunday
        };
        expected.SetHour(1, 1);
        using var context = Factory.CreateDbContext();
        var resultCtx =
            await UsageUpdateService.UpdateUsageStatisticsAsync<WashingMachine>(
                borrow, context);
        var result = resultCtx.WashingMachineUsage.ToList();
        Assert.Equal(expected, result[(int) IsoDayOfWeek.Sunday]);
    }

    [Fact]
    public async Task AddToUsageOverWeekTest()
    {
        var borrow = new Borrow();
        borrow.startDate =
            new LocalDateTime(2022, 8, 7, 5, 0).InUtc().ToInstant();
        using var context = Factory.CreateDbContext();
        var preValues = context.WashingMachineUsage.ToList();
        // Everything 1 except 3-7 for sunday == 0
        var expected = preValues.Select(
            prev =>
            {
                var ret = new BorrowableEntityUsage<WashingMachine>
                {
                    DayId = prev.DayId
                };
                if (ret.DayId == IsoDayOfWeek.None) return ret;
                Enumerable.Range(0, 24).ToList().ForEach(
                    hour => { ret.SetHour(hour, prev.GetHour(hour) + 1); }
                );
                return ret;
            }).ToList();
        // We start at 7 and end at 2 but we should take the 7 too
        Enumerable.Range(2, 5).ToList().ForEach(hour =>
        {
            var oldVal = expected[(int) IsoDayOfWeek.Sunday].GetHour(hour);
            expected[(int) IsoDayOfWeek.Sunday].SetHour(hour, oldVal - 1);
        });

        var resultCtx =
            await UsageUpdateService.UpdateUsageStatisticsAsync<WashingMachine>(
                borrow, context);
        var result = resultCtx.WashingMachineUsage.ToList();
        Assert.Equal(expected, result);
        // Don't save
    }
}