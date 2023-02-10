using App.Data.Constants;
using App.Data.Models;
using App.Data.ServiceInterfaces;
using Microsoft.EntityFrameworkCore;

namespace App.Data.EFCoreServices;

/// <summary>
/// EF Core implementation of <see cref="IUsageService"/>.
/// </summary>
public class UsageService : IUsageService
{
    private readonly IUsageConstants _usageConstants;
    private readonly IClock _clock;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clock"></param>
    /// <param name="usageConstants"></param>
    public UsageService(IClock clock,
        IUsageConstants usageConstants)
    {
        _clock = clock;
        _usageConstants = usageConstants;
    }

    /// <inheritdoc />
    public async Task<ApplicationDbContext> UpdateUsageStatisticsAsync<T>(
        Borrow borrow, ApplicationDbContext dbContext)
        where T : BorrowableEntity
    {
        var tz = _usageConstants.UsageTimeZone;
        var endDateInCet = _clock.GetCurrentInstant().InZone(tz);
        var startDateInCet = borrow.Start.InZone(tz);
        var duration =
            Period.DaysBetween(startDateInCet.Date, endDateInCet.Date);

        var usedIsoWeek = Enumerable.Range(0, Math.Min(duration + 1, 7))
            .Select(x => startDateInCet.Date.PlusDays(x).DayOfWeek).ToList();
        var foundDays = await dbContext.Set<BorrowableEntityUsage<T>>()
            .Where(x => usedIsoWeek.Contains(x.DayId))
            .ToListAsync();

        // Add new days if not in db
        var notInDBusage = usedIsoWeek.Except(foundDays.Select(x => x.DayId))
            .Select(x => new BorrowableEntityUsage<T>
            {
                DayId = x,
            }).ToArray();
        await dbContext.Set<BorrowableEntityUsage<T>>()
            .AddRangeAsync(notInDBusage);
        foundDays = foundDays.Concat(notInDBusage).ToList();


        long baseDays = duration / 7;
        foreach (var day in foundDays)
        {
            // 0 or 1
            long remainder = Enumerable.Range(0, (duration + 1) % 7)
                .Count(x =>
                    startDateInCet.Date.PlusDays(x).DayOfWeek == day.DayId);
            for (var i = 0; i < 24; i++)
            {
                long minus = 0;
                // if hour before start minus += 1
                if (i < startDateInCet.Hour &&
                    startDateInCet.DayOfWeek == day.DayId) minus += 1;

                // if hour after end minus += 1
                if (new LocalTime(i, 0) >= endDateInCet.LocalDateTime.TimeOfDay &&
                    endDateInCet.DayOfWeek == day.DayId) minus += 1;

                day.SetHour(i, day.GetHour(i) + baseDays + remainder - minus);
            }
        }

        return dbContext;
    }
}