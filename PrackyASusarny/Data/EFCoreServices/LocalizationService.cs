using PrackyASusarny.Data.ServiceInterfaces;

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

    public string GetLocalizedDate(LocalDate? date)
    {
        if (date == null) return string.Empty;

        return $"{date:dd/MM/yyyy}";
    }

    public int DecimalPlaces => 2;
}