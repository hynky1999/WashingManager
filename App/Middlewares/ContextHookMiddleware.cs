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

/// <inheritdoc />
public class ContextHookMiddleware : IContextHookMiddleware
{
    private readonly Dictionary<Type, EventHandler<object>?[]> _eventHandlers =
        new();


    /// <inheritdoc />
    public Task OnSave(EntityState state, object entity)
    {
        var type = entity.GetType();
        if (_eventHandlers.TryGetValue(type, out var handlers))
        {
            var handler = handlers[(int) state];
            handler?.Invoke(this, entity);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void AddContextHook<T>(EntityState state,
        Func<T, Task> hook) where T : class
    {
        if (!_eventHandlers.TryGetValue(typeof(T), out var handlers))
        {
            handlers =
                new EventHandler<object>[Enum.GetValues(typeof(EntityState))
                    .Length];
            _eventHandlers.Add(typeof(T), handlers);
        }

        handlers[(int) state] += (_, e) => hook((T) e);
    }
}