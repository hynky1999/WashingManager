namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ILocalizationService
{
    DateTimeZone TimeZone { get; }
    Instant Now { get; }
    ZonedDateTime NowInTimeZone { get; }

    int DecimalPlaces { get; }

    public string? this[string? key] { get; }
    public string? this[LocalDate? date] { get; }
    public string? this[LocalDateTime? date] { get; }

    public string? this[Instant? instant]{ get; }
    public string? this[ZonedDateTime? time]{ get; }
    public string? this[double? d]{ get; }
    public double? Round(double? d);
}