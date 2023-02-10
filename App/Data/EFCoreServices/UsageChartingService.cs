using App.Data.Constants;
using App.Data.Models;
using App.Data.ServiceInterfaces;
using Microsoft.EntityFrameworkCore;

namespace App.Data.EFCoreServices;

/// <summary>
/// EF Core implementation of <see cref="IUsageChartingService{T}"/>.
/// </summary>
public class UsageChartingService<T> : IUsageChartingService<T>
    where T : BorrowableEntity
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IUsageConstants _usageConstants;
    private readonly IClock _clock;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="usageConstants"></param>
    /// <param name="clock"></param>
    public UsageChartingService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IUsageConstants usageConstants, IClock clock)
    {
        _dbFactory = dbFactory;
        _usageConstants = usageConstants;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<(LocalTime hour, int value)[]> GetBorrowsByHourAsync(
        LocalDate date, DateTimeZone tz)
    {
        var startInstant = date. AtStartOfDayInZone(tz).ToInstant();
        var endInstant = date.PlusDays(1).AtStartOfDayInZone(tz).ToInstant();
        var context = await _dbFactory.CreateDbContextAsync();
        var dbset = context.Set<Borrow>();
        // We cannot just check on equality of day because of timezones
        var query = dbset.Where(borrow =>
            borrow.Start >= startInstant && borrow.Start < endInstant);
        query = query.Where(borrow => borrow.BorrowableEntity is T);
        var grouped = query.GroupBy(borrow => new
            {
                hour = borrow.Start
                    .InZone(DateTimeZoneProviders.Tzdb[tz.Id]).LocalDateTime
                    .Hour
            })
            .Select(group => new {group.Key.hour, count = group.Count()});
        var sqlResult =
            await grouped.ToDictionaryAsync(usage => usage.hour,
                usage => usage.count);
        var resultArray = Enumerable.Range(0, 24).Select(hour =>
        {
            var localeTime = new LocalTime(hour, 0);
            return (locale_date: localeTime,
                sqlResult.TryGetValue(hour, out var val) ? val : 0);
        }).ToArray();

        return resultArray;
    }


    /// <inheritdoc />
    public async Task<(LocalDate time, int value)[]> GetBorrowsByDayAsync(
        LocalDate start, LocalDate end, DateTimeZone tz)
    {
        
        
        if (end < start)
            throw new ArgumentException("End date is earlier than start date");

        var context = await _dbFactory.CreateDbContextAsync();
        // Don't just take date because of timezones
        var startInstant = start.AtStartOfDayInZone(tz).ToInstant();
        var endInstant = end.PlusDays(1).AtStartOfDayInZone(tz).ToInstant();
        var query = context.Borrows.Where(borrow =>
            borrow.Start >= startInstant && borrow.Start <= endInstant);
        query = query.Where(borrow => borrow.BorrowableEntity is T);
        var grouped = query.GroupBy(borrow => new
            {
                date = borrow.Start
                    .InZone(DateTimeZoneProviders.Tzdb[tz.Id]).LocalDateTime
                    .Date
            })
            .Select(group => new {group.Key.date, count = group.Count()});
        var sqlResult =
            await grouped.ToDictionaryAsync(usage => usage.date,
                usage => usage.count);

        var resultArray = Enumerable
            .Range(0, Period.DaysBetween(start, end) + 1).Select(
                offset =>
                {
                    var date = start.PlusDays(offset);
                    return (date,
                        sqlResult.TryGetValue(date, out var val) ? val : 0);
                }).ToArray();
        return resultArray;
    }

    /// <inheritdoc />
    public async Task<(IsoDayOfWeek dayOfWeek, double value)[]>
        GetWeekUsageAsync(DateTimeZone tz)
    {
        // TODO Support non-native
        if (tz != _usageConstants.UsageTimeZone)
            throw new ArgumentException("Non native tz currently unsupported");
            
            
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
        var sqlDict = await summed.ToDictionaryAsync(usage => usage.DayId,
            usage => usage.totalUsage);
        var mondaysSinceStart = (_clock.GetCurrentInstant()
                                    .InZone(_usageConstants.UsageTimeZone) - _usageConstants.CalculatedSince).TotalDays / 7.0;

        var resultArray = Enum.GetValues<IsoDayOfWeek>()
            .Where(val => val != IsoDayOfWeek.None).Select(day =>
                (day,
                    sqlDict.TryGetValue(day, out var val)
                        ? val / mondaysSinceStart
                        : 0)
            ).ToArray();
        return resultArray;
    }

    /// <inheritdoc />
    public async Task<(LocalTime hour, double value)[]> GetHourlyUsageAsync(
        IsoDayOfWeek dayOfWeek, DateTimeZone tz)
    {
        // TODO: Support non native timezones
        if (tz != _usageConstants.UsageTimeZone)
        {
            throw new ArgumentException("Non native currently timezone not supported");
        }
        var context = await _dbFactory.CreateDbContextAsync();
        var dbset = context.Set<BorrowableEntityUsage<T>>();
        var query = dbset.Where(entity => entity.DayId == dayOfWeek);
        var sqlResult = await query.FirstOrDefaultAsync();
        if (sqlResult == null)
            return Enumerable.Range(0, 24)
                .Select(hour => (new LocalTime(hour, 0), 0.0)).ToArray();
        //Qualified aproximation :)
        var mondaysSinceStart = (_clock.GetCurrentInstant()
                                    .InZone(_usageConstants.UsageTimeZone) - _usageConstants.CalculatedSince).TotalDays / 7.0;
        return Enumerable.Range(0, 24)
            .Select(hour => (new LocalTime(hour, 0),
                sqlResult.GetHour(hour) / mondaysSinceStart)).ToArray();
    }

    /// <inheritdoc />
    public async Task<(LocalTime hour, double value)[]> GetAvgHourlyUsageAsync(DateTimeZone tz)
    {
        // TODO: Support non native timezones
        if (tz != _usageConstants.UsageTimeZone)
        {
            throw new ArgumentException("Non native currently timezone not supported");
        }
        var context = await _dbFactory.CreateDbContextAsync();
        var dbset = context.Set<BorrowableEntityUsage<T>>();
        //Now idea how to make it nicer as Enumerable doesn't work in ef core query
        var query = dbset.GroupBy(p => 1).Select(usage =>
            new
            {
                sum = new[]
                {
                    usage.Sum(x => x.Hour0Total),
                    usage.Sum(x => x.Hour1Total),
                    usage.Sum(x => x.Hour2Total),
                    usage.Sum(x => x.Hour3Total),
                    usage.Sum(x => x.Hour4Total),
                    usage.Sum(x => x.Hour5Total),
                    usage.Sum(x => x.Hour6Total),
                    usage.Sum(x => x.Hour7Total),
                    usage.Sum(x => x.Hour8Total),
                    usage.Sum(x => x.Hour9Total),
                    usage.Sum(x => x.Hour10Total),
                    usage.Sum(x => x.Hour11Total),
                    usage.Sum(x => x.Hour12Total),
                    usage.Sum(x => x.Hour13Total),
                    usage.Sum(x => x.Hour14Total),
                    usage.Sum(x => x.Hour15Total),
                    usage.Sum(x => x.Hour16Total),
                    usage.Sum(x => x.Hour17Total),
                    usage.Sum(x => x.Hour18Total),
                    usage.Sum(x => x.Hour19Total),
                    usage.Sum(x => x.Hour20Total),
                    usage.Sum(x => x.Hour21Total),
                    usage.Sum(x => x.Hour22Total),
                    usage.Sum(x => x.Hour23Total)
                }
            });
        var sqlResult = await query.FirstOrDefaultAsync();
        if (sqlResult == null)
            return Enumerable.Range(0, 24)
                .Select(hour => (new LocalTime(hour, 0), 0.0)).ToArray();

        var mondaysSinceStart = (_clock.GetCurrentInstant()
                                    .InZone(_usageConstants.UsageTimeZone) - _usageConstants.CalculatedSince).TotalDays / 7.0;
        return sqlResult.sum.Select((sum, hour) =>
            (new LocalTime(hour, 0), sum / mondaysSinceStart)).ToArray();
    }
}