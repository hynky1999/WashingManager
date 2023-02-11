using Microsoft.EntityFrameworkCore;

namespace App.Middlewares;

/// <summary>
/// Interface for middleware which calls predefined hooks based on patterns.
/// It needs to be notified by OnSave to call the hooks.
/// </summary>
public interface IContextHookMiddleware
{
    /// <summary>
    /// When saving an entity, you can call this method to notify the middleware to call the hooks.
    /// </summary>
    /// <param name="state">The type of entity update</param>
    /// <param name="entity">entity itself</param>
    /// <returns></returns>
    public Task OnSave(EntityState state, object entity);

    /// <summary>
    /// Registers a hook for a specific entity type.
    /// </summary>
    /// <param name="state">State to react on</param>
    /// <param name="hook">Hook that takes entity and does something. It is fire and forget type.</param>
    /// <typeparam name="T"></typeparam>
    public void AddContextHook<T>(EntityState state,
        Func<T, Task> hook) where T : class;
}