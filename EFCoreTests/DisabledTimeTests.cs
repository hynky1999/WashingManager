using System.Linq;
using NodaTime;
using PrackyASusarny.Shared.Components.NodaComponents;
using Xunit;

namespace EFCoreTests;

public class DisabledTimeTests
{
    [Fact]
    public void TestDisabledTimeCreate()
    {
        var start = new LocalDateTime(2021, 1, 1, 5, 4, 0);
        var end = new LocalDateTime(2021, 1, 1, 9, 5, 0);
        var disbaled = DisabledTimesStore.FromNoda(start, end);
        var expectedHours = new int[] {6, 7, 8}.ToHashSet();
        var expectedMinutesStart = Enumerable.Range(4, 60 - 4).ToHashSet();
        var expectedMinutesEnd = Enumerable.Range(0, 5).ToHashSet();
        Assert.Equal(expectedHours, disbaled.DisabledHours);
        Assert.Equal(expectedMinutesStart, disbaled.DisabledMinutes[5]);
        Assert.Equal(expectedMinutesEnd, disbaled.DisabledMinutes[9]);
    }

    [Fact]
    public void TestDisabledDateTimeCreate()
    {
        var start = new LocalDateTime(2021, 1, 1, 5, 4, 0);
        var end = new LocalDateTime(2021, 1, 3, 9, 5, 0);
        var disbaled = DisabledDateTimesStore.FromNoda(start, end);
        var expectdDays = Enumerable.Range(1, 1)
            .Select(i => start.PlusDays(i).Date).ToHashSet();
        var expectedHoursStart = Enumerable.Range(6, 24 - 6).ToHashSet();
        var expectedHoursEnd = Enumerable.Range(0, 9).ToHashSet();
        Assert.Equal(expectdDays, disbaled.DisabledDates);
        Assert.Equal(expectedHoursStart,
            disbaled.DisabledTimes[start.Date].DisabledHours);
        Assert.Equal(expectedHoursEnd,
            disbaled.DisabledTimes[end.Date].DisabledHours);
    }

    [Fact]
    public void TestDisableDateTimeAdd()
    {
        var start1 = new LocalDateTime(2021, 1, 1, 0, 0, 0);
        var end1 = new LocalDateTime(2021, 1, 3, 9, 5, 0);
        var d1 = DisabledDateTimesStore.FromNoda(start1, end1);

        var start2 = new LocalDateTime(2021, 1, 3, 9, 5, 0);
        var end2 = new LocalDateTime(2021, 1, 6, 0, 0, 0);
        var d2 = DisabledDateTimesStore.FromNoda(start2, end2);
        d1.Add(d2);
        var expectdDays = Enumerable.Range(0, 5)
            .Select(i => start1.PlusDays(i).Date).ToHashSet();
        Assert.Equal(expectdDays, d1.DisabledDates);
    }
}