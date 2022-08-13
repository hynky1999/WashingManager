using PrackyASusarny.Data.EFCoreServices;

namespace PrackyASusarny.Data.Models;

public interface IModelWithService<T>
{
    public ICrudService<T> Service { get; }
}