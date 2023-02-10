using System.Globalization;
using App.Data.Constants;
using App.Data.ModelInterfaces;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Interface which manages localization of these areas:
/// - Localization of strings
/// - Localization of dates to current timezone and culture
/// - Localization of numbers to current culture
/// - Localization of currency to current culture
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Current timezone of the user
    /// </summary>
    DateTimeZone TimeZone { get; }

    /// <summary>
    /// Current time
    /// </summary>
    Instant Now { get; }

    /// <summary>
    /// Current time in the current timezone
    /// </summary>
    ZonedDateTime NowInTimeZone { get; }

    /// <summary>
    /// Possible Cultures to choose from
    /// </summary>
    public CultureInfo[] AvailableCultures { get; }

    /// <summary>
    /// Current culture
    /// </summary>
    public CultureInfo CurrentCulture { get; }

    // Exists solely for type-param classes
    /// <summary>
    /// Localizes an object based on it's runtime type
    /// Expect it to be slow and always use the overload with the type parameter
    /// </summary>
    /// <param name="key"></param>
    public string? this[object? key] { get; }

    /// <summary>
    /// String Loc
    /// </summary>
    /// <param name="key"></param>
    public string? this[string? key] { get; }

    /// <summary>
    /// Date Loc
    /// </summary>
    /// <param name="date"></param>
    public string? this[LocalDate? date] { get; }

    /// <summary>
    /// Date Loc
    /// </summary>
    public string? this[LocalDateTime? date] { get; }

    /// <summary>
    /// Date Loc
    /// </summary>
    public string? this[Instant? instant] { get; }

    /// <summary>
    /// Date Loc
    /// </summary>
    public string? this[ZonedDateTime? time] { get; }

    /// <summary>
    /// Date Loc
    /// </summary>
    public string? this[double? d] { get; }

    /// <summary>
    /// Localizes DB model description
    /// </summary>
    public string? this[IDBModel? model] { get; }

    /// <summary>
    /// Localizes Money to current currency
    /// </summary>
    public string? this[Money? money] { get; }

    /// <summary>
    /// Localizes a Enum by it's value as defined in Res table
    /// </summary>
    /// <param name="e"></param>
    public string? this[Enum e] { get; }

    /// <summary>
    /// Rounds to common number of decimal places
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public double? Round(double? d);
}