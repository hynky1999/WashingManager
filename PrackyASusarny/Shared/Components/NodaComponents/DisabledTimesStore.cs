using AntDesign;

namespace PrackyASusarny.Shared.Components.NodaComponents;

/// <summary>
/// Class that represents disabled time periods.
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

    public HashSet<int> DisabledHours { get; private set; }
    public Dictionary<int, HashSet<int>> DisabledMinutes { get; private set; }

    public bool isFullDayDisabled()
    {
        return DisabledHours.Count == 24;
    }

    /// <summary>
    /// Converts all 0-59 minutes to disabled hour.
    /// </summary>
    public void ToUniformState()
    {
        foreach ((var hour, var minutes) in DisabledMinutes)
        {
            if (minutes.Count == 60)
            {
                DisabledHours.Add(hour);
                DisabledMinutes.Remove(hour);
            }
        }
    }

    public DatePickerDisabledTime AsDatePickerDisabledTime(LocalDateTime dt)
    {
        var hours = DisabledHours;
        DisabledMinutes.TryGetValue(dt.Hour, out var maybeMinutes);
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
/// </summary>
public class DisabledDateTimesStore
{
    public DisabledDateTimesStore(HashSet<LocalDate> disabledDates,
        Dictionary<LocalDate, DisabledTimesStore> disabledTimes)
    {
        DisabledDates = disabledDates;
        DisabledTimes = disabledTimes;
        ToUniformState();
    }

    // Dictionary so search is fast
    public HashSet<LocalDate> DisabledDates { get; private set; }

    public Dictionary<LocalDate, DisabledTimesStore> DisabledTimes
    {
        get;
        private set;
    }

    /// <summary>
    /// Converts disabled time period which represents full day to disabled date.
    /// </summary>
    public void ToUniformState()
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

    public DatePickerDisabledTime AsDatePickerDisabledTime(LocalDateTime dt)
    {
        if (DisabledDates.Contains(dt.Date))
        {
            return NodaUtils.AllDisabled;
        }

        if (DisabledTimes.TryGetValue(dt.Date, out var disabledDate))
        {
            return disabledDate.AsDatePickerDisabledTime(dt);
        }

        return NodaUtils.AllAllowed;
    }

    public bool DisabledDate(LocalDate dt)
    {
        return DisabledDates.Contains(dt);
    }

    public static DisabledDateTimesStore FromNodaRanges(
        IEnumerable<(LocalDateTime, LocalDateTime)> ranges)
    {
        var disabled = new DisabledDateTimesStore(new HashSet<LocalDate>(),
            new Dictionary<LocalDate, DisabledTimesStore>());

        foreach (var (start, end) in ranges)
        {
            disabled.Add(FromNoda(start, end));
        }

        return disabled;
    }

    public static DisabledDateTimesStore Empty()
    {
        return new DisabledDateTimesStore(new HashSet<LocalDate>(),
            new Dictionary<LocalDate, DisabledTimesStore>());
    }
}