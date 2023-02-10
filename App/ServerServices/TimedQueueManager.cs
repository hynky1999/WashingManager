using System.Collections.Concurrent;

namespace App.ServerServices;

/// <inheritdoc />
public class TimedQueueManager<TItem> : ITimedQueueManager<TItem>
{
    private readonly Func<int, ITimedQueue<TItem>> _factory;
    private readonly ConcurrentDictionary<int, ITimedQueue<TItem>> _items;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="factory"></param>
    public TimedQueueManager(Func<int, ITimedQueue<TItem>> factory)
    {
        _items = new();
        _factory = factory;
    }


    /// <inheritdoc />
    public ITimedQueue<TItem> CreateQueue(int id)
    {
        var queue = _factory(id);
        _items.TryAdd(id, queue);
        return queue;
    }

    /// <inheritdoc />
    public ITimedQueue<TItem>? GetQueue(int id)
    {
        return _items.TryGetValue(id, out var queue) ? queue : null;
    }

    /// <inheritdoc />
    public void RemoveQueue(int id)
    {
        _items.TryRemove(id, out var queue);
        queue?.Dispose();
    }

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var queue in _items.Values)
        {
            queue.Dispose();
        }
    }
}

/// <summary>
/// A class which manages multiple ITimedQueue instances
/// It enables to create, get and remove queues
/// </summary>
/// <typeparam name="TItem"></typeparam>
public interface ITimedQueueManager<TItem> : IDisposable
{
    /// <summary>
    /// Returns number of queues
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Creates a new queue
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ITimedQueue<TItem> CreateQueue(int id);

    /// <summary>
    /// Returns a queue with given id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ITimedQueue<TItem>? GetQueue(int id);

    /// <summary>
    /// Removes a queue with given id
    /// </summary>
    /// <param name="id"></param>
    public void RemoveQueue(int id);
}