namespace App.Data.Constants;

/// <inheritdoc />
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

/// <summary>
/// Contans for calculating usage (Average since last reset)
/// </summary>
public interface IUsageConstants
{
    /// <summary>
    /// Since when we collected data
    /// </summary>
    public ZonedDateTime CalculatedSince { get; }

    /// <summary>
    /// What time zone is usage saved in
    /// </summary>
    public DateTimeZone UsageTimeZone { get; }
}