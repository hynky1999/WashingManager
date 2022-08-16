namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ICrudService<T>
{
    public new Task<List<T>> GetAllAsync(bool eager);
    public new Task<T?> GetByIdAsync(object id, bool eager);
    public Task CreateAsync(T entity);
    public Task UpdateAsync(T entity);
    public new Task DeleteAsync(T entity);
    public object GetId(T entity);
}