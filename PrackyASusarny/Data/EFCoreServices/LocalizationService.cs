namespace PrackyASusarny.Data.EFCoreServices;

public class LocalizationService : ILocalizationService
{
    private readonly IClock _clock;

    public LocalizationService(IClock clock)
    {
        _clock = clock;
    }

    public DateTimeZone TimeZone { get; } = DateTimeZoneProviders.Tzdb["Europe/Prague"];
    public Instant Now => _clock.GetCurrentInstant();
    public ZonedDateTime NowInTimeZone => _clock.GetCurrentInstant().InZone(TimeZone);

    public int DecimalPlaces => 2;
}