using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moq;
using NodaTime;
using PrackyASusarny.Data;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.LocServices;
using PrackyASusarny.Data.Models;
using Xunit;

namespace EFCoreTests;

// Since we want to use the same db for all tests, we need parameterless constructor
public class _TEST_DB_USAGE : DBFullFactory
{
    public _TEST_DB_USAGE() : base("usage_test")
    {
    }

    protected override void FillData(ApplicationDbContext context)
    {
        base.FillData(context);
        context.SaveChanges();
        context.AddRange(CreateUsages());
        context.AddRange(CreateBorrows());
    }

    private List<BorrowableEntityUsage<WashingMachine>> CreateUsages()
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

    private List<Borrow> CreateBorrows()
    {
        var borrows = new List<Borrow>
        {
            new()
            {
                BorrowableEntityID = 1,
                Start = Instant.FromUtc(2020, 1, 1, 1, 0),
                BorrowPersonID = 1,
                End = null, xmin = 0
            },
            new()
            {
                BorrowableEntityID = 1,
                Start = Instant.FromUtc(2020, 1, 2, 0, 0),
                BorrowPersonID = 1,
                End = null, xmin = 0
            },
            new()
            {
                BorrowableEntityID = 1,
                Start = Instant.FromUtc(2020, 1, 2, 5, 0),
                BorrowPersonID = 1,
                End = null, xmin = 0
            },
            new()
            {
                BorrowableEntityID = 1,
                Start = Instant.FromUtc(2020, 1, 2, 5, 3),
                BorrowPersonID = 1,
                End = null, xmin = 0
            },

            new()
            {
                BorrowableEntityID = 1,
                Start = Instant.FromUtc(2020, 1, 4, 10, 0),
                BorrowPersonID = 1,
                End = null, xmin = 0
            }
        };
        return borrows;
    }
}

public class ChartingTests : IClassFixture<_TEST_DB_USAGE>
{
    private readonly double _mondaysSinceStart;

    public ChartingTests(_TEST_DB_USAGE factory)
    {
        CurrencyService currencyService = new CurrencyService();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        UsageConstants = new TestUsageConstants();
        var localization =
            new LocalizationService(new Utils.Clock14082022(), currencyService,
                Mock.Of<IStringLocalizer<LocalizationService>>(),
                configuration);
        UsageService =
            new UsageChartingService<WashingMachine>(factory,
                UsageConstants, localization);
        UsageUpdateService = new UsageService(localization, UsageConstants);
        Factory = factory;
        using var context = Factory.CreateDbContext();
        context.SaveChanges();
        _mondaysSinceStart = 6 / 7.0;
    }

    private IUsageConstants UsageConstants { get; }


    private UsageChartingService<WashingMachine> UsageService { get; }
    private DBFullFactory Factory { get; }
    private UsageService UsageUpdateService { get; }

    private bool EqUsage<T>(BorrowableEntityUsage<T> u1,
        BorrowableEntityUsage<T> u2) where T : BorrowableEntity
    {
        for (int i = 0; i < 24; i++)
        {
            if (u1.GetHour(i) != u2.GetHour(i))
            {
                return false;
            }
        }

        return true;
    }


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
        expected[1].Item2 = 1;
        expected[6].Item2 = 2;
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
        borrow.Start =
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
        Assert.True(EqUsage(expected, result[(int) IsoDayOfWeek.Sunday]));
    }

    [Fact]
    public async Task AddToUsageOverWeekTest()
    {
        var borrow = new Borrow();
        borrow.Start =
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
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.True(EqUsage(expected[i], result[i]));
        }
        // Don't save
    }

    private class TestUsageConstants : IUsageConstants
    {
        public ZonedDateTime CalculatedSince =>
            new(new LocalDateTime(2022, 8, 8, 0, 0), DateTimeZone.Utc,
                Offset.Zero);

        public DateTimeZone UsageTimeZone =>
            DateTimeZoneProviders.Tzdb["Europe/Prague"];
    }
}