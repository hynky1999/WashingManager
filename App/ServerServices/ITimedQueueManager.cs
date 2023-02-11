namespace App.ServerServices;

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