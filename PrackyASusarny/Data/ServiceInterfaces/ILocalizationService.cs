using PrackyASusarny.Data.ModelInterfaces;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ILocalizationService
{
    DateTimeZone TimeZone { get; }
    Instant Now { get; }
    ZonedDateTime NowInTimeZone { get; }

    // Exists solely for typeparam classes
    public string? this[object? key] { get; }
    public string? this[string? key] { get; }
    public string? this[LocalDate? date] { get; }
    public string? this[LocalDateTime? date] { get; }

    public string? this[Instant? instant] { get; }
    public string? this[ZonedDateTime? time] { get; }
    public string? this[double? d] { get; }
    public string? this[IDBModel? model] { get; }

    public string? this[params string?[] keys] { get; }
    public double? Round(double? d);
}