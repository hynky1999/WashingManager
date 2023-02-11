namespace App.ServerServices;

/// <inheritdoc />
public class DBTimedQueue<TItem> : ITimedQueue<TItem>
{
    private readonly Func<TItem, Duration> _getDuration;
    private readonly Func<Task<TItem?>> _getNextEvent;
    private Timer? _timer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="getNextEvent"></param>
    /// <param name="getDuration"></param>
    public DBTimedQueue(Func<Task<TItem?>> getNextEvent,
        Func<TItem, Duration> getDuration)
    {
        _getNextEvent = getNextEvent;
        _getDuration = getDuration;
        _timer = null;
    }

    /// <inheritdoc />
    public event EventHandler<TItem>? OnEvent;

    /// <inheritdoc />
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

    /// <inheritdoc />
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