using Microsoft.EntityFrameworkCore;

namespace App.Middlewares;

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