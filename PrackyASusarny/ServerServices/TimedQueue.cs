namespace PrackyASusarny.ServerServices;

/// <summary>
/// ITimedQueue represents queue of objects that will be processed in specific time.
/// The queue will plan a timer whenever a OnChange is called, the event with shorted duration will be selected.
/// To process this event you need to subscribe to the OnTimer event.
/// Bear in mind that when Timer is invoked it will not automatically requeue another one (Call OnChange).
/// You have to call OnChange manually in event handler.
/// </summary>
/// <typeparam name="TItem"></typeparam>
public interface ITimedQueue<TItem> : IDisposable
{
    public event EventHandler<TItem>? OnEvent;
    public Task OnChange();
}

public class DBTimedQueue<TItem> : ITimedQueue<TItem>
{
    private readonly Func<TItem, Duration> _getDuration;
    private readonly Func<Task<TItem?>> _getNextEvent;
    private Timer? _timer;

    public DBTimedQueue(Func<Task<TItem?>> getNextEvent,
        Func<TItem, Duration> getDuration)
    {
        _getNextEvent = getNextEvent;
        _getDuration = getDuration;
        _timer = null;
    }

    public event EventHandler<TItem>? OnEvent;

    public async Task OnChange()
    {
        // If we have a timer running destroy it
        if (_timer != null)
        {
            await _timer.DisposeAsync();
        }

        var nextEvent = await _getNextEvent();
        // No next event again null check because of warning
        if (EqualityComparer<TItem>.Default.Equals(nextEvent, default(TItem)) ||
            nextEvent == null)
        {
            return;
        }

        var timeToEvent = _getDuration(nextEvent);
        if (timeToEvent < Duration.Zero)
        {
            timeToEvent = Duration.Zero;
        }

        // + 1 millisecond to be sure it will be called after event
        var timeToNextEvent = ((Int64) timeToEvent.TotalMilliseconds) + 1;
        // Else the Timer will not be created
        if (timeToNextEvent > Int32.MaxValue)
        {
            timeToNextEvent = Int32.MaxValue;
        }

        var newTimer = new Timer(OnTimer, nextEvent, timeToNextEvent,
            Timeout.Infinite);
        _timer = newTimer;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        foreach (var handler in OnEvent?.GetInvocationList() ??
                                Array.Empty<Delegate>())
        {
            OnEvent -= (EventHandler<TItem>) handler;
        }
    }

    private void OnTimer(object? state)
    {
        var nextEvent = (TItem) state!;
        if (!EqualityComparer<TItem>.Default.Equals(nextEvent,
                default(TItem)) &&
            _getDuration(nextEvent) <= Duration.Zero)
        {
            OnEvent?.Invoke(this, nextEvent);
        }
    }
}