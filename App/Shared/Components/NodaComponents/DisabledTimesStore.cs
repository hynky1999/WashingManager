using AntDesign;

namespace App.Shared.Components.NodaComponents;

/// <summary>
/// Class that represents disabled time periods(One day).
/// The class stores minutes and hours, seconds are ignored.
/// </summary>
public class DisabledTimesStore
{
    private DisabledTimesStore(HashSet<int> disabledHours,
        Dictionary<int, HashSet<int>> disabledMinutes)
    {
        DisabledHours = disabledHours;
        DisabledMinutes = disabledMinutes;
        ToUniformState();
    }

    /// <summary>
    /// Gets the disabled hours of the class.
    /// </summary>
    public HashSet<int> DisabledHours { get; private set; }

    /// <summary>
    /// Gets the disabled minutes of the class.
    /// </summary>
    public Dictionary<int, HashSet<int>> DisabledMinutes { get; private set; }

    /// <summary>
    /// Checks if class represents a full disabled day.
    /// </summary>
    /// <returns></returns>
    public bool isFullDayDisabled()
    {
        return DisabledHours.Count == 24;
    }

    /// <summary>
    /// Converts all 0-59 minutes to disabled hour.
    /// </summary>
    public void ToUniformState()
    {
        foreach (var (hour, minutes) in DisabledMinutes)
        {
            if (minutes.Count == 60)
            {
                DisabledHours.Add(hour);
                DisabledMinutes.Remove(hour);
            }
        }
    }

    /// <summary>
    /// Converts itself to DatePickerDisabledTime, minutes are chosen based on the hour.
    /// </summary>
    /// <param name="hour"></param>
    /// <returns>DatePickerDisabledTime</returns>
    public DatePickerDisabledTime AsDatePickerDisabledTimeInHour(int hour)
    {
        var hours = DisabledHours;
        DisabledMinutes.TryGetValue(hour, out var maybeMinutes);
        var minutes = maybeMinutes ?? new HashSet<int>();
        var seconds = Array.Empty<int>();
        return new DatePickerDisabledTime(
            hours.ToArray(),
            minutes.ToArray(),
            seconds
        );
    }

    /// <summary>
    ///  Add disabled time period to this period.
    ///  It will merge minute eg: 0..23, 24..59 -> will become disabled hour.
    /// </summary>
    /// <param name="other">
    /// Disabled time period to add.
    /// </param>
    public void AddDisabledTime(DisabledTimesStore other)
    {
        DisabledHours.UnionWith(other.DisabledHours);
        foreach (var key in other.DisabledMinutes.Keys)
        {
            if (DisabledMinutes.TryGetValue(key, out var minutes))
            {
                minutes.UnionWith(other.DisabledMinutes[key]);
            }
            else
            {
                DisabledMinutes[key] = other.DisabledMinutes[key];
            }
        }

        ToUniformState();
    }

    /// <summary>
    /// Creates itself from noda time period. The period must be in one day.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="excludeEnd"></param>
    /// <returns>representation of disabled time based on period span</returns>
    /// <exception cref="ArgumentException">If period is over one day or start is before end</exception>
    public static DisabledTimesStore FromNoda(LocalDateTime start,
        LocalDateTime end, bool excludeEnd = true)
    {
        if (start.Date != end.Date || start > end)
        {
            throw new ArgumentException(
                "Must be the same date and start < end");
        }

        var hours = end.Hour - start.Hour;
        // Not start or end hours
        var disabledMinutes = new Dictionary<int, HashSet<int>>();
        var excludeEndAdd = excludeEnd ? 0 : 1;
        HashSet<int> disabledHours = new();
        // Same hour
        if (hours == 0)
        {
            disabledMinutes.Add(start.Hour,
                Enumerable.Range(start.Minute,
                        end.Minute - start.Minute + excludeEndAdd)
                    .ToHashSet());
        }
        else
        {
            disabledHours = Enumerable.Range(start.Hour + 1, hours - 1)
                .ToHashSet();
            // Start hour
            disabledMinutes.Add(start.Hour,
                Enumerable.Range(start.Minute, 60 - start.Minute).ToHashSet());
            // End hour
            disabledMinutes.Add(end.Hour,
                Enumerable.Range(0, end.Minute + excludeEndAdd).ToHashSet());
        }

        return new DisabledTimesStore(
            disabledHours,
            disabledMinutes
        );
    }
}

/// <summary>
/// Store disabled time periods Date + hours + minutes. Second are ignored.
/// The representation is very simple. It keeps an HasheSet of fully disabled dates,
/// for not fully disabled dates it uses <see cref="DisabledTimesStore"/>
/// </summary>
public class DisabledDateTimesStore
{
    /// <summary>
    /// </summary>
    /// <param name="disabledDates"></param>
    /// <param name="disabledTimes"></param>
    private DisabledDateTimesStore(HashSet<LocalDate> disabledDates,
        Dictionary<LocalDate, DisabledTimesStore> disabledTimes)
    {
        DisabledDates = disabledDates;
        DisabledTimes = disabledTimes;
        ToUniformState();
    }

    // Dictionary so search is fast
    /// <summary>
    ///  Gets the disabled dates of the class.
    /// </summary>
    public HashSet<LocalDate> DisabledDates { get; private set; }

    /// <summary>
    /// Gets the disabled times of the class.
    /// </summary>
    public Dictionary<LocalDate, DisabledTimesStore> DisabledTimes
    {
        get;
        private set;
    }

    /// <summary>
    /// Converts disabled time period which represents full day to disabled date.
    /// </summary>
    private void ToUniformState()
    {
        foreach (var key in DisabledTimes.Keys)
        {
            DisabledTimes[key].ToUniformState();
            if (DisabledTimes[key].isFullDayDisabled())
            {
                DisabledDates.Add(key);
                DisabledTimes.Remove(key);
            }
        }
    }

    /// <summary>
    /// Creates itself from noda time period.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="excludeEnd"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DisabledDateTimesStore FromNoda(LocalDateTime start,
        LocalDateTime end, bool excludeEnd = true)
    {
        if (start > end)
        {
            throw new ArgumentException("Must be start < end");
        }

        var days = Period.DaysBetween(start.Date, end.Date);
        // Not start or end
        var disabledDays = Enumerable.Range(1, days - 1)
            .Select(x => start.Date.PlusDays(x)).ToHashSet();


        var dTimes = new Dictionary<LocalDate, DisabledTimesStore>();
        // Same date
        if (days == 0)
        {
            dTimes.Add(start.Date,
                DisabledTimesStore.FromNoda(start, end,
                    excludeEnd));
        }
        else
        {
            // Start date
            dTimes.Add(start.Date,
                DisabledTimesStore.FromNoda(start,
                    start.Date.At(new LocalTime(23, 59, 59)),
                    excludeEnd: false));

            dTimes.Add(end.Date,
                DisabledTimesStore.FromNoda(
                    end.Date.AtMidnight(), end, excludeEnd));
        }

        return new DisabledDateTimesStore(
            disabledDays, dTimes
        );
    }

    /// <summary>
    /// Inplace add disabled time period to this period.
    /// </summary>
    /// <param name="other"></param>
    public void Add(DisabledDateTimesStore other)
    {
        DisabledDates.UnionWith(other.DisabledDates);

        foreach (var (key, val) in other.DisabledTimes)
        {
            if (DisabledTimes.TryGetValue(key, out var disabledDate))
            {
                disabledDate.AddDisabledTime(val);
            }
            else
            {
                DisabledTimes.Add(key, val);
            }
        }

        ToUniformState();
    }

    /// <summary>
    /// Converts a date to DatePickerDisabledTime.
    /// Similar to <see cref="DisabledTimesStore.AsDatePickerDisabledTimeInHour(int)"/>
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public DatePickerDisabledTime AsDatePickerDisabledTime(LocalDateTime dt)
    {
        if (DisabledDates.Contains(dt.Date))
        {
            return NodaUtils.AllDisabled;
        }

        if (DisabledTimes.TryGetValue(dt.Date, out var disabledDate))
        {
            return disabledDate.AsDatePickerDisabledTimeInHour(dt.Hour);
        }

        return NodaUtils.AllAllowed;
    }

    /// <summary>
    /// Checks if the class is fully disabled on given date.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public bool DisabledDate(LocalDate dt)
    {
        return DisabledDates.Contains(dt);
    }

    /// <summary>
    /// Creates a disabled time period from a list Noda ranges.
    /// </summary>
    /// <param name="ranges"></param>
    /// <returns>Disabled time period representing ranges</returns>
    public static DisabledDateTimesStore FromNodaRanges(
        IEnumerable<(LocalDateTime, LocalDateTime)> ranges)
    {
        var disabled = Empty();

        foreach (var (start, end) in ranges)
        {
            disabled.Add(FromNoda(start, end));
        }

        return disabled;
    }

    /// <summary>
    /// Returns an DisabledDateTimesStore which represents no disabled time = all allowed.
    /// </summary>
    /// <returns>All allowed disDateTimesStore</returns>
    public static DisabledDateTimesStore Empty()
    {
        return new DisabledDateTimesStore(new HashSet<LocalDate>(),
            new Dictionary<LocalDate, DisabledTimesStore>());
    }
}