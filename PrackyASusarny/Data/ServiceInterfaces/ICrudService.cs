namespace PrackyASusarny.Data.EFCoreServices;

public interface ICrudService<T>
{
    public Task<List<T>> GetAllAsync();
    public Task<T?> GetByIdAsync(int id);
    public void CreateAsync(T entity);
    public void UpdateAsync(T entity);
    public void DeleteAsync(int id);
}