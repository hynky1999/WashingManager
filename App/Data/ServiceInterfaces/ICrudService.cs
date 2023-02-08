using AntDesign.TableModels;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Interface for the service that provides generic access to the database.
/// It provides methods for CRUD operations.
/// It always does not checks or validates the data.
/// It doesn't do any authorization of the current user.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICrudService<T>
{
    /// <summary>
    /// Gets all items of the type T based on queryModel filters.
    /// </summary>
    /// <param name="queryModel"></param>
    /// <param name="eager">Should all relates tables be fetched too ?</param>
    /// <returns>All items of type T</returns>
    public Task<T[]> GetAllAsync(QueryModel<T>? queryModel = null,
        bool eager = false);

    /// <summary>
    /// <see cref="GetAllAsync"/> but counts the items.
    /// </summary>
    /// <param name="queryModel"></param>
    /// <returns>Count of all items</returns>
    public Task<int> GetCountAsync(QueryModel<T>? queryModel);

    /// <summary>
    /// Returns the item with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="eager">Should all relates tables be fetched too ?</param>
    /// <returns>Returns item if found otherwise null</returns>
    public Task<T?> GetByIdAsync(object id, bool eager = false);

    /// <summary>
    /// Create an item in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>Item created</returns>
    public Task<T> CreateAsync(T entity);

    /// <summary>
    /// Update an item in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>Updated item</returns>
    public Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Delete an item from the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public Task DeleteAsync(T entity);

    /// <summary>
    /// Returns a key/ID of the item.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>key/ID</returns>
    public object GetId(T entity);
}