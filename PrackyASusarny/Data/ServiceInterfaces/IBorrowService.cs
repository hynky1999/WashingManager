using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IBorrowService
{
    Task<Price> GetPriceAsync(Borrow borrow);
    Task<Borrow?> GetBorrowByWmAsync(WashingMachine wm);
    Task EndBorrowAsync(Borrow borrow);
    Task<(DateTime time, double value)> GetUsageByHourAsync(object id, DateTime day);
    Task<(DateTime time, double value)> GetUsageByHourAllAsync(DateTime day);
    Task<(DateTime time, double value)> GetUsageByDayAsync(object id, DateTime start, DateTime? end);
    Task<(DateTime time, double value)> GetUsageByDayAllAsync(DateTime start, DateTime? end);
}

public interface IWashingMachineStatisticsService : ICrudService<WashingMachineStatistics>
{
    // Gets Usage Statistic for a single Washing Machine
    // Gets Usage Statistic for a all Washing Machines
    public (string label, double value) GetStatisticUsageAll(StatRange range);
}

public enum StatRange
{
    ByHour,
    ByDay
}