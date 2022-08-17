using System.Linq.Expressions;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IWashingMachineService
{
    public Expression<Func<WashingMachine, bool>> FloorRangeFilter { get; }
    public Expression<Func<WashingMachine, bool>> StatusFilter { get; }
    public Expression<Func<WashingMachine, bool>> BuildingFilter { get; }
}