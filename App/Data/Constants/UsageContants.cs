namespace App.Data.Constants;


/// <summary>
/// Implementation of usage constants for the application
/// </summary>
public class UsageContants : IUsageConstants
{
    /// <inheritdoc />
    public ZonedDateTime CalculatedSince =>
        new(new LocalDateTime(2023, 1, 24, 0, 0), DateTimeZone.Utc,
            Offset.Zero);


    /// <inheritdoc />
    public DateTimeZone UsageTimeZone =>
        DateTimeZoneProviders.Tzdb["Europe/Prague"];
}