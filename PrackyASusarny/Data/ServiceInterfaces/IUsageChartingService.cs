using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IUsageChartingService
{
    public Task<(LocalTime hour, int value)[]> GetBorrowsByHourAsync(LocalDate day);

    public Task<(LocalDate time, int value)[]> GetBorrowsByDayAsync(LocalDate start,
        LocalDate end);

    // Int as hour makes sense since it is fixed timezone(+1) thus converting to other ones doesn't make sense
    public Task<(LocalTime hour, double value)[]> GetHourlyUsageAsync(IsoDayOfWeek dayOfWeek);
    public Task<(LocalTime hour, double value)[]> GetAvgHourlyUsageAsync();
    public Task<(IsoDayOfWeek dayOfWeek, double value)[]> GetWeekUsageAsync();
}

public interface IUsageChartingService<T> : IUsageChartingService where T : BorrowableEntity
{
}