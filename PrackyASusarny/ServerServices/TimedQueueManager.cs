using System.Collections.Concurrent;

namespace PrackyASusarny.ServerServices;

public class TimedQueueManager<TItem> : ITimedQueueManager<TItem>
{
    private readonly Func<int, ITimedQueue<TItem>> _factory;
    private readonly ConcurrentDictionary<int, ITimedQueue<TItem>> _items;

    public TimedQueueManager(Func<int, ITimedQueue<TItem>> factory)
    {
        _items = new();
        _factory = factory;
    }


    public ITimedQueue<TItem> CreateQueue(int id)
    {
        var queue = _factory(id);
        _items.TryAdd(id, queue);
        return queue;
    }

    public ITimedQueue<TItem>? GetQueue(int id)
    {
        return _items.TryGetValue(id, out var queue) ? queue : null;
    }

    public void RemoveQueue(int id)
    {
        _items.TryRemove(id, out var queue);
        queue?.Dispose();
    }

    public int Count => _items.Count;

    public void Dispose()
    {
        foreach (var queue in _items.Values)
        {
            queue.Dispose();
        }
    }
}

public interface ITimedQueueManager<TItem> : IDisposable
{
    public int Count { get; }
    public ITimedQueue<TItem> CreateQueue(int id);
    public ITimedQueue<TItem>? GetQueue(int id);
    public void RemoveQueue(int id);
}