namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IUsageChartingService
{
    // Int as hour makes sense since it is fixed timezone thus converting to other ones doesn't make sense
    public Task<(DateTime hour, int value)[]> GetBorrowsByHourAsync(DateTime day);

    public Task<(DateTime time, int value)[]> GetBorrowsByDayAsync(DateTime start,
        DateTime end);

    public Task<(int hour, double value)[]> GetHourlyUsageAsync(DayOfWeek dayOfWeek);
    public Task<(int hour, double value)[]> GetAvgHourlyUsageAsync();
    public Task<(DayOfWeek dayOfWeek, double value)[]> GetWeekUsageAsync();
}