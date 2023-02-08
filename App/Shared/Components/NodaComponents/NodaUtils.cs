using AntDesign;
using App.Data.ServiceInterfaces;
using NodaTime.Extensions;

namespace App.Shared.Components.NodaComponents;

/// <summary>
/// Extension methods for conversion between NodaTime and DateTime
/// It is used mostly by DatePicker components as AntDesign uses DateTime
/// So we had to convert it to NodaTime
/// </summary>
public static class NodaUtils
{
    /// <summary>
    /// Defines DatePickerDisabledTime with all dates allowed
    /// </summary>
    public static readonly DatePickerDisabledTime AllAllowed =
        new DatePickerDisabledTime(Array.Empty<int>(), Array.Empty<int>(),
            Array.Empty<int>());

    /// <summary>
    /// Defines DatePickerDisabledTime with all dates disabled
    /// </summary>
    public static readonly DatePickerDisabledTime AllDisabled =
        new DatePickerDisabledTime(Enumerable.Range(0, 24).ToArray(),
            Enumerable.Range(0, 60).ToArray(),
            Enumerable.Range(0, 60).ToArray());

    /// <summary>
    /// Converts DateTime to Instant
    /// DateTime timezone is ignored and is treated as local time
    /// </summary>
    /// <param name="dateTime">Local time</param>
    /// <param name="localizationService"></param>
    /// <returns>Instant representing dateTime based on app timezone</returns>
    public static Instant ToInstant(DateTime dateTime,
        ILocalizationService localizationService)
    {
        return dateTime.ToLocalDateTime()
            .InZoneLeniently(localizationService.TimeZone).ToInstant();
    }

    /// <summary>
    /// Converts DateTime? to Instant?
    /// DateTime timezone is ignored and is treated as local time
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="localizationService"></param>
    /// <returns>Instant? representing dateTime based on app timezone</returns>
    public static Instant? ToInstant(DateTime? dateTime,
        ILocalizationService localizationService)
    {
        if (dateTime == null)
        {
            return null;
        }

        return ToInstant(dateTime.Value, localizationService);
    }

    /// <summary>
    /// Converts Instant to DateTime
    /// DateTime represents local time based on app timezone
    /// </summary>
    /// <param name="instant"></param>
    /// <param name="localizationService"></param>
    /// <returns>DateTime</returns>
    public static DateTime ToDateTime(Instant instant,
        ILocalizationService localizationService)
    {
        return instant.InZone(localizationService.TimeZone)
            .ToDateTimeUnspecified();
    }


    /// <summary>
    /// Converts Instant? to DateTime?
    /// DateTime represents local time based on app timezone
    /// </summary>
    /// <param name="val"></param>
    /// <param name="localizationService"></param>
    /// <returns>DateTime</returns>
    public static DateTime? ToDateTime(Instant? val,
        ILocalizationService localizationService)
    {
        if (val is null) return null;
        return ToDateTime(val.Value, localizationService);
    }


    /// <summary>
    /// Converts DateTime to LocalDate
    /// TimeZone of DateTime is ignored
    /// </summary>
    /// <param name="val"></param>
    /// <returns>LocalDate</returns>
    public static LocalDate ToLocalDate(DateTime val)
    {
        return val.ToLocalDateTime().Date;
    }

    /// <summary>
    /// Converts DateTime? to LocalDate?
    /// TimeZone of DateTime is ignored
    /// </summary>
    /// <param name="val"></param>
    /// <returns>LocalDate?</returns>
    public static LocalDate? ToLocalDate(DateTime? val)
    {
        if (val is null) return null;

        return ToLocalDate(val.Value);
    }

    /// <summary>
    /// Converts LocalDate to DateTime, the timezone is set to unspecified
    /// as it represents local date
    /// </summary>
    /// <param name="val"></param>
    /// <returns>DateTime</returns>
    public static DateTime ToDateTime(LocalDate val)
    {
        return val.ToDateTimeUnspecified();
    }

    /// <summary>
    /// Converts LocalDate to DateTime, the timezone is set to unspecified
    /// as it represents local date
    /// </summary>
    /// <param name="val"></param>
    /// <returns>DateTime?</returns>
    public static DateTime? ToDateTime(LocalDate? val)
    {
        if (val is null) return null;
        return ToDateTime(val.Value);
    }

    /// <summary>
    /// Converts DateTime to LocalDateTime
    /// TimeZone of DateTime is ignored
    /// </summary>
    /// <param name="val"></param>
    /// <returns>LocalDateTime</returns>
    public static LocalDateTime ToLocalDateTime(DateTime val)
    {
        return val.ToLocalDateTime();
    }

    /// <summary>
    /// Converts DateTime? to LocalDateTime?
    /// TimeZone of DateTime is ignored
    /// </summary>
    /// <param name="val"></param>
    /// <returns>LocalDateTime</returns>
    public static LocalDateTime? ToLocalDateTime(DateTime? val)
    {
        if (val is null) return null;

        return ToLocalDateTime(val.Value);
    }


    /// <summary>
    /// Converts LocalDateTime to DateTime, the timezone is set to unspecified
    /// </summary>
    /// <param name="val"></param>
    /// <returns>DateTime</returns>
    public static DateTime ToDateTime(LocalDateTime val)
    {
        return val.ToDateTimeUnspecified();
    }

    /// <summary>
    /// Converts LocalDateTime to DateTime?, the timezone is set to unspecified
    /// </summary>
    /// <param name="val"></param>
    /// <returns>DateTime?</returns>
    public static DateTime? ToDateTime(LocalDateTime? val)
    {
        if (val is null) return null;

        return ToDateTime(val.Value);
    }
}