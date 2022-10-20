namespace PrackyASusarny.Data.EFCoreServices;

public interface ILocalizationService
{
    DateTimeZone TimeZone { get; }
    Instant Now { get; }
    ZonedDateTime NowInTimeZone { get; }

    int DecimalPlaces { get; }
}