using System.Globalization;
using App.Data.Constants;
using App.Data.ModelInterfaces;
using App.Data.ServiceInterfaces;
using Microsoft.Extensions.Localization;
using NodaTime.Text;

#pragma warning disable CS1591

namespace App.Data.LocServices;

/// <summary>
/// Implementation of <see cref="ILocalizationService"/>
/// with IStringLocalize.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IClock _clock;
    private readonly ICurrencyService _currencyService;
    private readonly IStringLocalizer<LocalizationService> _localizer;
    private readonly Currency _userCurrency = Currency.CZK;

    private readonly int DecimalPlaces = 2;

    public LocalizationService(IClock clock, ICurrencyService currencyService,
        IStringLocalizer<LocalizationService> localizer,
        IConfiguration configuration)
    {
        _clock = clock;
        _currencyService = currencyService;
        _localizer = localizer;
        AvailableCultures = configuration.GetSection("Cultures").GetChildren()
            .ToDictionary(x => x.Key, x => x.Value)
            .Select(x => new CultureInfo(x.Value)).ToArray();
    }

    public DateTimeZone TimeZone { get; } =
        DateTimeZoneProviders.Tzdb["Europe/Prague"];

    public Instant Now => _clock.GetCurrentInstant();

    public ZonedDateTime NowInTimeZone =>
        _clock.GetCurrentInstant().InZone(TimeZone);

    public CultureInfo[] AvailableCultures { get; }

    public CultureInfo CurrentCulture => CultureInfo.CurrentCulture;


    public string? this[string? key] =>
        key == null ? null : _localizer[key].Value;

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

    public string? this[Enum? e] => this[e?.ToString()];

    public double? Round(double? d)
    {
        if (d == null) return null;
        return Math.Round(d.Value, DecimalPlaces);
    }

    public string? this[Instant? instant] => this[instant?.InZone(TimeZone)];

    /// <summary>
    /// Approximates the value of the given amount in the user's currency.
    /// We can't use async calls thus we use approximation.
    /// </summary>
    /// <param name="money"></param>
    public string? this[Money? money] =>
        money != null
            ? _currencyService.ApproximateTo(money, _userCurrency).ToString()
            : null;

    public string? this[IDBModel? model] => model?.HumanReadableLoc(this);

    /// <summary>
    /// Returns the localized name of the given currency.
    /// !!! IT's very slow !!!
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="NotImplementedException">If the object is not supported</exception>
    public string? this[object? key]
    {
        get
        {
            if (key == null) return null;
            if (key is Enum e) return this[e];
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