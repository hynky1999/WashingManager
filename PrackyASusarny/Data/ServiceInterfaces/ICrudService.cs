using System.Linq.Expressions;
using PrackyASusarny.Data.EFCoreServices;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ICrudService<T>
{
    public Task<List<TResult>> GetAllAsync<TResult, TKey>(Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>[]? filters = null, SortOption<T, TKey>[]? sortKeys = null, bool eager = false);

    public Task<List<T>> GetAllAsync(Expression<Func<T, bool>>[]? filters = null, bool eager = false);

    public Task<TResult?> GetByIdAsync<TResult>(object id, Expression<Func<T, TResult>> selector,
        bool eager = false);

    public Task<T?> GetByIdAsync(object id, bool eager = false);
    public Task CreateAsync(T entity);
    public Task UpdateAsync(T entity);
    public Task DeleteAsync(T entity);
    public object GetId(T entity);
}