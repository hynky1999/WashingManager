namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ILocalizationService
{
    DateTimeZone TimeZone { get; }
    Instant Now { get; }
    ZonedDateTime NowInTimeZone { get; }

    int DecimalPlaces { get; }

    string GetLocalizedDate(LocalDate? date);
}