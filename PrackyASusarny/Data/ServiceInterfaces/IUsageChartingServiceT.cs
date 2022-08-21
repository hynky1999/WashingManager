using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IUsageChartingService<T> : IUsageChartingService where T : BorrowableEntity
{
}