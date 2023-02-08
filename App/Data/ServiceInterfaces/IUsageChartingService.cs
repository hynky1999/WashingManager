using App.Data.Models;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Interface for the service that handles querying UsageData for charting
/// </summary>
public interface IUsageChartingService
{
    /// <summary>
    /// Returns n. Borrows per hour in specified date.
    /// We use LocalTime as frontend is in local time.
    /// </summary>
    /// <param name="day"></param>
    /// <returns>Borrows per hour</returns>
    public Task<(LocalTime hour, int value)[]> GetBorrowsByHourAsync(
        LocalDate day);

    /// <summary>
    /// Returns n. Borrows per day in range start-end.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns>Borrows per day</returns>
    public Task<(LocalDate time, int value)[]> GetBorrowsByDayAsync(
        LocalDate start,
        LocalDate end);

    /// <summary>
    /// Gets average number of borrows per hour in specified day of week.
    /// </summary>
    /// <param name="dayOfWeek"></param>
    /// <returns>Average number of borrows</returns>
    public Task<(LocalTime hour, double value)[]> GetHourlyUsageAsync(
        IsoDayOfWeek dayOfWeek);

    /// <summary>
    /// Gets average number of borrows per hour over all of week days.
    /// </summary>
    /// <returns>Average number of borrows</returns>
    public Task<(LocalTime hour, double value)[]> GetAvgHourlyUsageAsync();

    /// <summary>
    /// Gets average number of borrows per weekday.
    /// </summary>
    /// <returns>Average number of borrows</returns>
    public Task<(IsoDayOfWeek dayOfWeek, double value)[]> GetWeekUsageAsync();
}

// ReSharper disable once UnusedTypeParameter
/// <inheritdoc />
public interface IUsageChartingService<T> : IUsageChartingService
    where T : BorrowableEntity
{
}