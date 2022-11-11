using NodaTime.Text;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class LocalizationService : ILocalizationService
{
    private readonly IClock _clock;

    public LocalizationService(IClock clock)
    {
        _clock = clock;
    }

    public DateTimeZone TimeZone { get; } =
        DateTimeZoneProviders.Tzdb["Europe/Prague"];

    public Instant Now => _clock.GetCurrentInstant();

    public ZonedDateTime NowInTimeZone =>
        _clock.GetCurrentInstant().InZone(TimeZone);
    
    public string? this[string? key] => key;

    public string? this[LocalDate? date]
    {
        get {
            if (date == null) return null;
            return LocalDatePattern.CreateWithCurrentCulture("d")
                .Format(date.Value);
        }
    }

    public string? this[LocalDateTime? date]
    {
        get {
            if (date == null) return null;
            return LocalDateTimePattern.CreateWithCurrentCulture("g")
                .Format(date.Value);
        }
    }

    public string? this[ZonedDateTime? time] => this[time?.LocalDateTime];

    public string? this[double? d] => Round(d).ToString();

    public double? Round(double? d)
    {
        if (d == null) return null;
        return Math.Round(d.Value, DecimalPlaces);
    }

    public string? this[Instant? instant] => this[instant?.InZone(TimeZone)];

    public int DecimalPlaces => 2;
}