namespace PrackyASusarny.Data.Constants;

public class UsageContants : IUsageConstants
{
    public ZonedDateTime CalculatedSince =>
        new(new LocalDateTime(2023, 1, 24, 0, 0), DateTimeZone.Utc,
            Offset.Zero);
}

public interface IUsageConstants
{
    public ZonedDateTime CalculatedSince { get; }
}