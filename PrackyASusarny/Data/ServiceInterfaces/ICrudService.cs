using AntDesign.TableModels;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ICrudService<T>
{
    public Task<T[]> GetAllAsync(QueryModel<T>? queryModel = null,
        bool eager = false);

    public Task<int> GetCountAsync(QueryModel<T>? queryModel);

    public Task<T?> GetByIdAsync(object id, bool eager = false);
    public Task<T> CreateAsync(T entity);
    public Task<T> UpdateAsync(T entity);
    public Task DeleteAsync(T entity);
    public object GetId(T entity);
}