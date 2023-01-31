using AntDesign;
using NodaTime.Extensions;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Shared.Components.NodaComponents;

public static class NodaUtils
{
    public static DatePickerDisabledTime AllAllowed =
        new DatePickerDisabledTime(Array.Empty<int>(), Array.Empty<int>(),
            Array.Empty<int>());

    public static DatePickerDisabledTime AllDisabled =
        new DatePickerDisabledTime(Enumerable.Range(0, 24).ToArray(),
            Enumerable.Range(0, 60).ToArray(),
            Enumerable.Range(0, 60).ToArray());

    public static Instant ToInstant(DateTime dateTime,
        ILocalizationService localizationService)
    {
        return dateTime.ToLocalDateTime()
            .InZoneLeniently(localizationService.TimeZone).ToInstant();
    }

    public static Instant? ToInstant(DateTime? dateTime,
        ILocalizationService localizationService)
    {
        if (dateTime == null)
        {
            return null;
        }

        return ToInstant(dateTime, localizationService);
    }

    public static DateTime ToDateTime(Instant instant,
        ILocalizationService localizationService)
    {
        return instant.InZone(localizationService.TimeZone)
            .ToDateTimeUnspecified();
    }

    public static DateTime? ToDateTime(Instant? val,
        ILocalizationService localizationService)
    {
        if (val is null) return null;
        return ToDateTime(val.Value, localizationService);
    }


    public static LocalDate ToLocalDate(DateTime val)
    {
        return val.ToLocalDateTime().Date;
    }

    public static LocalDate? ToLocalDate(DateTime? val)
    {
        if (val is null) return null;

        return ToLocalDate(val.Value);
    }

    public static DateTime ToDateTime(LocalDate val)
    {
        return val.ToDateTimeUnspecified();
    }

    public static DateTime? ToDateTime(LocalDate? val)
    {
        if (val is null) return null;
        return ToDateTime(val.Value);
    }

    public static LocalDateTime ToLocalDateTime(DateTime val)
    {
        return val.ToLocalDateTime();
    }

    public static LocalDateTime? ToLocalDateTime(DateTime? val)
    {
        if (val is null) return null;

        return ToLocalDateTime(val.Value);
    }


    public static DateTime ToDateTime(LocalDateTime val)
    {
        return val.ToDateTimeUnspecified();
    }

    public static DateTime? ToDateTime(LocalDateTime? val)
    {
        if (val is null) return null;

        return ToDateTime(val.Value);
    }
}