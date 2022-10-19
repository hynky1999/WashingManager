using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class UsageChartingService<T> : IUsageChartingService<T> where T : BorrowableEntity
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILocalizationService _localizationService;

    public UsageChartingService(IDbContextFactory<ApplicationDbContext> dbFactory,
        ILocalizationService localizationService)
    {
        _dbFactory = dbFactory;
        _localizationService = localizationService;
    }

    public async Task<(LocalTime hour, int value)[]> GetBorrowsByHourAsync(LocalDate date)
    {
        var tz = _localizationService.TimeZone;
        var startInstant = date.AtStartOfDayInZone(tz).ToInstant();
        var endInstant = date.PlusDays(1).AtStartOfDayInZone(tz).ToInstant();
        var context = await _dbFactory.CreateDbContextAsync();
        var dbset = context.Set<Borrow>();
        // We cannot just check on equality of day because of timezones
        var query = dbset.Where(borrow => borrow.startDate >= startInstant && borrow.startDate < endInstant);
        var grouped = query.GroupBy(borrow => new
                {hour = borrow.startDate.InZone(DateTimeZoneProviders.Tzdb[tz.Id]).LocalDateTime.Hour})
            .Select(group => new {group.Key.hour, count = group.Count()});
        var sqlResult = await grouped.ToDictionaryAsync(usage => usage.hour, usage => usage.count);
        var resultArray = Enumerable.Range(0, 24).Select(hour =>
        {
            var localeTime = new LocalTime(hour, 0);
            return (locale_date: localeTime, sqlResult.TryGetValue(hour, out var val) ? val : 0);
        }).ToArray();

        return resultArray;
    }


    public async Task<(LocalDate time, int value)[]> GetBorrowsByDayAsync(LocalDate start, LocalDate end)
    {
        if (end < start) throw new ArgumentException("End date is earlier than start date");

        var context = await _dbFactory.CreateDbContextAsync();
        // Don't just take date because of timezones
        var tz = _localizationService.TimeZone;
        var startInstant = start.AtStartOfDayInZone(tz).ToInstant();
        var endInstant = end.PlusDays(1).AtStartOfDayInZone(tz).ToInstant();
        var query = context.Borrows.Where(borrow =>
            borrow.startDate >= startInstant && borrow.startDate <= endInstant);
        var grouped = query.GroupBy(borrow => new
                {date = borrow.startDate.InZone(DateTimeZoneProviders.Tzdb[tz.Id]).LocalDateTime.Date})
            .Select(group => new {group.Key.date, count = group.Count()});
        var sqlResult = await grouped.ToDictionaryAsync(usage => usage.date, usage => usage.count);

        var resultArray = Enumerable.Range(0, Period.DaysBetween(start, end) + 1).Select(
            offset =>
            {
                var date = start.PlusDays(offset);
                return (date, sqlResult.TryGetValue(date, out var val) ? val : 0);
            }).ToArray();
        return resultArray;
    }

    public async Task<(IsoDayOfWeek dayOfWeek, double value)[]> GetWeekUsageAsync()
    {
        var context = await _dbFactory.CreateDbContextAsync();
        var query = context.Set<BorrowableEntityUsage<T>>();
        var summed = query.Select(usage => new
        {
            usage.DayId, totalUsage = usage.Hour0Total
                                      + usage.Hour1Total
                                      + usage.Hour2Total
                                      + usage.Hour3Total
                                      + usage.Hour4Total
                                      + usage.Hour5Total
                                      + usage.Hour6Total
                                      + usage.Hour7Total
                                      + usage.Hour8Total
                                      + usage.Hour9Total
                                      + usage.Hour10Total
                                      + usage.Hour11Total
                                      + usage.Hour12Total
                                      + usage.Hour13Total
                                      + usage.Hour14Total
                                      + usage.Hour15Total
                                      + usage.Hour16Total
                                      + usage.Hour17Total
                                      + usage.Hour18Total
                                      + usage.Hour19Total
                                      + usage.Hour20Total
                                      + usage.Hour21Total
                                      + usage.Hour22Total
                                      + usage.Hour23Total
        });
        var sqlDict = await summed.ToDictionaryAsync(usage => usage.DayId, usage => usage.totalUsage);
        double mondaysSinceStart = Period.DaysBetween(BorrowableEntityUsage.CalculatedSince.Date,
            _localizationService.NowInTimeZone.Date) / 7.0;

        var resultArray = Enum.GetValues<IsoDayOfWeek>().Where(val => val != IsoDayOfWeek.None).Select(day =>
            (day, sqlDict.TryGetValue(day, out var val) ? val / mondaysSinceStart : 0)
        ).ToArray();
        return resultArray;
    }

    public async Task<(LocalTime hour, double value)[]> GetHourlyUsageAsync(IsoDayOfWeek dayOfWeek)
    {
        var context = await _dbFactory.CreateDbContextAsync();
        var dbset = context.Set<BorrowableEntityUsage<T>>();
        var query = dbset.Where(entity => entity.DayId == dayOfWeek);
        var sqlResult = await query.FirstOrDefaultAsync();
        if (sqlResult == null) return Array.Empty<(LocalTime hour, double value)>();

        //Qualified aproximation :)
        double mondaysSinceStart = Period.DaysBetween(BorrowableEntityUsage.CalculatedSince.Date,
            _localizationService.NowInTimeZone.Date) / 7.0;
        return EntityUsageToStat(sqlResult, 1 / mondaysSinceStart);
    }

    public async Task<(LocalTime hour, double value)[]> GetAvgHourlyUsageAsync()
    {
        var context = await _dbFactory.CreateDbContextAsync();
        var dbset = context.Set<BorrowableEntityUsage<T>>();
        var query = dbset.GroupBy(p => 1).Select(usage =>
            new
            {
                hour0 = usage.Sum(x => x.Hour0Total),
                hour1 = usage.Sum(x => x.Hour1Total),
                hour2 = usage.Sum(x => x.Hour2Total),
                hour3 = usage.Sum(x => x.Hour3Total),
                hour4 = usage.Sum(x => x.Hour4Total),
                hour5 = usage.Sum(x => x.Hour5Total),
                hour6 = usage.Sum(x => x.Hour6Total),
                hour7 = usage.Sum(x => x.Hour7Total),
                hour8 = usage.Sum(x => x.Hour8Total),
                hour9 = usage.Sum(x => x.Hour9Total),
                hour10 = usage.Sum(x => x.Hour10Total),
                hour11 = usage.Sum(x => x.Hour11Total),
                hour12 = usage.Sum(x => x.Hour12Total),
                hour13 = usage.Sum(x => x.Hour13Total),
                hour14 = usage.Sum(x => x.Hour14Total),
                hour15 = usage.Sum(x => x.Hour15Total),
                hour16 = usage.Sum(x => x.Hour16Total),
                hour17 = usage.Sum(x => x.Hour17Total),
                hour18 = usage.Sum(x => x.Hour18Total),
                hour19 = usage.Sum(x => x.Hour19Total),
                hour20 = usage.Sum(x => x.Hour20Total),
                hour21 = usage.Sum(x => x.Hour21Total),
                hour22 = usage.Sum(x => x.Hour22Total),
                hour23 = usage.Sum(x => x.Hour23Total)
            });
        var sqlResult = await query.FirstOrDefaultAsync();
        if (sqlResult == null) return Array.Empty<(LocalTime hour, double value)>();

        double mondaysSinceStart = Period.DaysBetween(BorrowableEntityUsage.CalculatedSince.Date,
            _localizationService.NowInTimeZone.Date) / 7.0;
        return EntityUsageToStat(new BorrowableEntityUsage<T>
        {
            Hour0Total = sqlResult.hour0,
            Hour1Total = sqlResult.hour1,
            Hour2Total = sqlResult.hour2,
            Hour3Total = sqlResult.hour3,
            Hour4Total = sqlResult.hour4,
            Hour5Total = sqlResult.hour5,
            Hour6Total = sqlResult.hour6,
            Hour7Total = sqlResult.hour7,
            Hour8Total = sqlResult.hour8,
            Hour9Total = sqlResult.hour9,
            Hour10Total = sqlResult.hour10,
            Hour11Total = sqlResult.hour11,
            Hour12Total = sqlResult.hour12,
            Hour13Total = sqlResult.hour13,
            Hour14Total = sqlResult.hour14,
            Hour15Total = sqlResult.hour15,
            Hour16Total = sqlResult.hour16,
            Hour17Total = sqlResult.hour17,
            Hour18Total = sqlResult.hour18,
            Hour19Total = sqlResult.hour19,
            Hour20Total = sqlResult.hour20,
            Hour21Total = sqlResult.hour21,
            Hour22Total = sqlResult.hour22,
            Hour23Total = sqlResult.hour23
        }, 1 / mondaysSinceStart);
    }

    private (LocalTime hour, double value)[] EntityUsageToStat(BorrowableEntityUsage e, double mod)
    {
        var resultArray = new (int hour, double value)[]
        {
            (0, e.Hour0Total * mod),
            (1, e.Hour1Total * mod),
            (2, e.Hour2Total * mod),
            (3, e.Hour3Total * mod),
            (4, e.Hour4Total * mod),
            (5, e.Hour5Total * mod),
            (6, e.Hour6Total * mod),
            (7, e.Hour7Total * mod),
            (8, e.Hour8Total * mod),
            (9, e.Hour9Total * mod),
            (10, e.Hour10Total * mod),
            (11, e.Hour11Total * mod),
            (12, e.Hour12Total * mod),
            (13, e.Hour13Total * mod),
            (14, e.Hour14Total * mod),
            (15, e.Hour15Total * mod),
            (16, e.Hour16Total * mod),
            (17, e.Hour17Total * mod),
            (18, e.Hour18Total * mod),
            (19, e.Hour19Total * mod),
            (20, e.Hour20Total * mod),
            (21, e.Hour21Total * mod),
            (22, e.Hour22Total * mod),
            (23, e.Hour23Total * mod)
        };
        var localTimed = resultArray.Select(x => (new LocalTime(x.hour, 0), x.value)).ToArray();

        return localTimed;
    }
}