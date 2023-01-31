using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class UsageService : IUsageService
{
    private readonly ILocalizationService _localizationService;

    public UsageService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public async Task<ApplicationDbContext> UpdateUsageStatisticsAsync<T>(
        Borrow borrow, ApplicationDbContext dbContext)
        where T : BorrowableEntity
    {
        var tz = BorrowableEntityUsage.TimeZone();
        var endDateInCet = _localizationService.Now.InZone(tz);
        var startDateInCet = borrow.startDate.InZone(tz);
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
            long remainder = Enumerable.Range(0, (duration + 1) % 7)
                .Count(x =>
                    startDateInCet.Date.PlusDays(x).DayOfWeek == day.DayId);
            for (var i = 0; i < 24; i++)
            {
                long minus = 0;
                if (i < startDateInCet.Hour &&
                    startDateInCet.DayOfWeek == day.DayId) minus += 1;

                if (i >= endDateInCet.Hour &&
                    endDateInCet.DayOfWeek == day.DayId) minus += 1;

                day.SetHour(i, day.GetHour(i) + baseDays + remainder - minus);
            }
        }

        return dbContext;
    }
}