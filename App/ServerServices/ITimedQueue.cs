namespace App.ServerServices;

/// <summary>
/// ITimedQueue represents queue of objects that will be processed in specific time.
/// The queue will plan a timer whenever a OnChange is called, the event with shortest duration will be selected.
/// To process this event you need to subscribe to the OnTimer event.
/// Bear in mind that when Timer is invoked it will not automatically requeue another one (Call OnChange).
/// You have to call OnChange manually in event handler.
/// </summary>
/// <typeparam name="TItem"></typeparam>
public interface ITimedQueue<TItem> : IDisposable
{
    /// <summary>
    /// Event that will be invoked when next event is ready to be processed.
    /// </summary>
    public event EventHandler<TItem>? OnEvent;

    /// <summary>
    /// Find next event to queue.
    /// </summary>
    /// <returns></returns>
    public Task OnChange();
}