using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.Models;
using Xunit;

namespace EFCoreTests;

// Since we want to use the same db for all tests, we need parameterless constructor
public class _TEST_DB_USAGE : DBFullFactory
{
    public _TEST_DB_USAGE() : base("usage_test")
    {
    }
}

public class ChartingTests : IClassFixture<_TEST_DB_USAGE>
{
    private readonly double _mondaysSinceStart;

    public ChartingTests(_TEST_DB_USAGE factory)
    {
        CurrencyService currencyService = new CurrencyService();
        var localization =
            new LocalizationService(new Utils.Clock14082022(), currencyService);
        UsageService =
            new UsageChartingService<WashingMachine>(factory,
                new MyUsageConstants(), localization);
        UsageUpdateService = new UsageService(localization);
        Factory = factory;

        _mondaysSinceStart = 6 / 7.0;
    }

    private UsageChartingService<WashingMachine> UsageService { get; }
    private DBFullFactory Factory { get; }
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

    private class MyUsageConstants : IUsageConstants
    {
        public ZonedDateTime CalculatedSince => new(
            new LocalDateTime(2022, 8, 8, 0, 0), DateTimeZone.Utc, Offset.Zero);
    }
}