namespace App.Data.Constants;

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