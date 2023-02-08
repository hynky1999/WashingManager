using System.ComponentModel.DataAnnotations;
using System.Reflection;

#pragma warning disable CS1591

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace App.Data.Models;

/// <summary>
/// Represents usage of a Borrowable Entity
/// This means how many times a Borrowable Entity was borrowed in certain hours
/// The data collected are per Weekday and hour
/// </summary>
public abstract class BorrowableEntityUsage
{
    /// <summary>
    /// ID of the day of the week only (1-7) will be created
    /// </summary>
    [Key]
    public IsoDayOfWeek DayId { get; set; }

    // Ugly but faster than having array since since won't change
    // The hours are based on CET Timezone ! (Unlike all other dates which are in UTC)
    public long Hour0Total { get; set; }
    public long Hour1Total { get; set; }
    public long Hour2Total { get; set; }
    public long Hour3Total { get; set; }
    public long Hour4Total { get; set; }
    public long Hour5Total { get; set; }
    public long Hour6Total { get; set; }
    public long Hour7Total { get; set; }
    public long Hour8Total { get; set; }
    public long Hour9Total { get; set; }
    public long Hour10Total { get; set; }
    public long Hour11Total { get; set; }
    public long Hour12Total { get; set; }
    public long Hour13Total { get; set; }
    public long Hour14Total { get; set; }
    public long Hour15Total { get; set; }
    public long Hour16Total { get; set; }
    public long Hour17Total { get; set; }
    public long Hour18Total { get; set; }
    public long Hour19Total { get; set; }
    public long Hour20Total { get; set; }
    public long Hour21Total { get; set; }
    public long Hour22Total { get; set; }
    public long Hour23Total { get; set; }


    private PropertyInfo hourToProperty(int hour)
    {
        var p = GetType().GetProperty($"Hour{hour}Total")!;
        return p;
    }

    /// <summary>
    /// Sets hour in usage to given value
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="value"></param>
    public void SetHour(int hour, long value)
    {
        hourToProperty(hour).SetValue(this, value);
    }

    /// <summary>
    /// Gets hour in usage
    /// </summary>
    /// <param name="hour"></param>
    /// <returns>hour in usage</returns>
    public long GetHour(int hour)
    {
        return (long) hourToProperty(hour).GetValue(this)!;
    }
}

// ReSharper disable once UnusedTypeParameter
// We need parameter be able to use drying room and wm ones
/// <summary>
/// Represents a usage of a borrowable entity of type T
/// Since we need a table per type, we need generic type
/// </summary>
/// <typeparam name="T">Type of borrowable entity</typeparam>
public class BorrowableEntityUsage<T> : BorrowableEntityUsage
    where T : BorrowableEntity
{
}