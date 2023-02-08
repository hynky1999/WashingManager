using App.Data.Models;
using App.Data.ServiceInterfaces;
using App.Features.Charts.Components;

namespace App.Features.Charts;

/// <summary>
/// Class Defines what models we allow to be used in the charts.
/// It also defines what chart we will create for each model.
/// </summary>
public static class ChartsConfig
{
    /// <summary>
    /// Allowed models for charting
    /// </summary>
    public static readonly Type[] AllowedChartModels =
        {typeof(WashingMachine), typeof(DryingRoom)};

    /// <summary>
    /// Defines chart type
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="loc"></param>
    /// <returns>
    /// Name of chart, type of chart and params to call init type with
    /// </returns>
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