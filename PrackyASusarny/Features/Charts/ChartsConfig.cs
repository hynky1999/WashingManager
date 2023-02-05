using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Features.Charts.Components;

namespace PrackyASusarny.Features.Charts;

public static class ChartsConfig
{
    public static readonly Type[] AllowedChartModels =
        {typeof(WashingMachine), typeof(DryingRoom)};

    public static (string name, Type type, Dictionary<string, object> args)[]
        GetChartsConfig(Type entityType,
            ILocalizationService loc)
    {
        return new[]
        {
            ("Daily borrows",
                typeof(DailyBorrows<>).MakeGenericType(entityType),
                new Dictionary<string, object>
                {
                    {"SinceDate", loc.NowInTimeZone.Date.PlusDays(-10)},
                    {"ToDate", loc.NowInTimeZone.Date}
                }
            ),
            ("Hourly borrows",
                typeof(HourlyBorrows<>).MakeGenericType(entityType),
                new Dictionary<string, object>
                {
                    {"Date", loc.NowInTimeZone.Date}
                }
            ),
            ("Weekly usage", typeof(WeeklyUsage<>).MakeGenericType(entityType),
                new Dictionary<string, object>()),
            ("Average hourly usage",
                typeof(AvgHourlyUsage<>).MakeGenericType(entityType),
                new Dictionary<string, object>()),
            ("Hourly usage", typeof(HourlyUsage<>).MakeGenericType(entityType),
                new Dictionary<string, object> {{"Day", DayOfWeek.Monday}})
        };
    }
}