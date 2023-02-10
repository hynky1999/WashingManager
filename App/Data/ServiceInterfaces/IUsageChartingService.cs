using App.Data.Models;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Interface for the service that handles querying UsageData for charting
/// </summary>
public interface IUsageChartingService
{
    /// <summary>
    /// Returns n. Borrows per hour in specified date.
    /// While the arguments might seems strange, this is the only way to
    /// enforce getting just date and timezone from the user.
    /// </summary>
    /// <param name="day"></param>
    /// <param name="tz"></param>
    /// <returns>Borrows per hour</returns>
    public Task<(LocalTime hour, int value)[]> GetBorrowsByHourAsync(
        LocalDate day, DateTimeZone tz);

    /// <summary>
    /// Returns n. Borrows per day in range start-end.
    /// While the arguments might seems strange, this is the only way to
    /// enforce getting just date and timezone from the user.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="tz"></param>
    /// <returns>Borrows per day</returns>
    public Task<(LocalDate time, int value)[]> GetBorrowsByDayAsync(
        LocalDate start,
        LocalDate end,
        DateTimeZone tz);

    /// <summary>
    /// Gets average number of borrows per hour in specified day of week.
    /// </summary>
    /// <param name="dayOfWeek"></param>
    /// <param name="tz"></param>
    /// <returns>Average number of borrows</returns>
    public Task<(LocalTime hour, double value)[]> GetHourlyUsageAsync(
        IsoDayOfWeek dayOfWeek, DateTimeZone tz);

    /// <summary>
    /// Gets average number of borrows per hour over all of week days.
    /// In specified timezone.
    /// </summary>
    /// <param name="tz"></param>
    /// <returns>Average number of borrows</returns>
    public Task<(LocalTime hour, double value)[]> GetAvgHourlyUsageAsync(DateTimeZone tz);

    /// <summary>
    /// Gets average number of borrows per weekday.
    /// In specified timezone.
    /// </summary>
    /// <param name="tz"></param>
    /// <returns>Average number of borrows</returns>
    public Task<(IsoDayOfWeek dayOfWeek, double value)[]> GetWeekUsageAsync(DateTimeZone tz);
}

// ReSharper disable once UnusedTypeParameter
/// <inheritdoc />
public interface IUsageChartingService<T> : IUsageChartingService
    where T : BorrowableEntity
{
}