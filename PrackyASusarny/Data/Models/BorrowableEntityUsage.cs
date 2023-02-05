using System.ComponentModel.DataAnnotations;
using System.Reflection;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace PrackyASusarny.Data.Models;

public abstract class BorrowableEntityUsage
{
    [Key] public IsoDayOfWeek DayId { get; set; }

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


    public PropertyInfo hourToProperty(int hour)
    {
        var p = GetType().GetProperty($"Hour{hour}Total")!;
        return p;
    }

    public void SetHour(int hour, long value)
    {
        hourToProperty(hour).SetValue(this, value);
    }

    public long GetHour(int hour)
    {
        return (long) hourToProperty(hour).GetValue(this)!;
    }
}

// ReSharper disable once UnusedTypeParameter
// We need parameter be able to use drying room and wm ones
public class BorrowableEntityUsage<T> : BorrowableEntityUsage
    where T : BorrowableEntity
{
}