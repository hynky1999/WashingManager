using NodaTime.Extensions;
using PrackyASusarny.Data.EFCoreServices;

namespace PrackyASusarny.Shared.Components.NodaComponents;

public static class NodaUtils
{
    public static Instant DateTimeToInstant(DateTime dateTime, ILocalizationService localizationService)
    {
        return dateTime.ToLocalDateTime().InZoneLeniently(localizationService.TimeZone).ToInstant();
    }

    public static DateTime InstantToDateTime(Instant instant, ILocalizationService localizationService)
    {
        return instant.InZone(localizationService.TimeZone).ToDateTimeUnspecified();
    }

    public static LocalDate? DateTimeToLocalDate(DateTime? val)
    {
        if (val is null)
        {
            return null;
        }

        return val.Value.ToLocalDateTime().Date;
    }

    public static DateTime? LocalDateToDateTime(LocalDate? val)
    {
        if (val is null)
        {
            return null;
        }

        return val.Value.ToDateTimeUnspecified();
    }
}