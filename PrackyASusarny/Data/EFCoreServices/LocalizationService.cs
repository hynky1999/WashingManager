using NodaTime.Text;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class LocalizationService : ILocalizationService
{
    private readonly IClock _clock;
    private int DecimalPlaces = 2;

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
        get
        {
            if (date == null) return null;
            return LocalDatePattern.CreateWithCurrentCulture("d")
                .Format(date.Value);
        }
    }

    public string? this[LocalDateTime? date]
    {
        get
        {
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

    public string? this[params string?[] keys]
    {
        get { return string.Join(" ", keys.Select(k => this[k])); }
    }

    public string? this[IDBModel? model] => model?.HumanReadableLoc(this);

    public string? this[object? key]
    {
        get
        {
            if (key == null) return null;
            if (key is string s) return this[s];
            if (key is LocalDate ld) return this[ld];
            if (key is LocalDateTime ldt) return this[ldt];
            if (key is ZonedDateTime zdt) return this[zdt];
            if (key is Instant i) return this[i];
            if (key is double d) return this[d];
            if (key is IDBModel m) return this[m];
            throw new NotImplementedException();
        }
    }
}