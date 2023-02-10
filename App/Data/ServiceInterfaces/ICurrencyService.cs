using App.Data.Constants;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Service for working with currencies and currency rates
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Converts money to specified currency
    /// </summary>
    /// <param name="money"></param>
    /// <param name="toCurrency"></param>
    /// <returns>Money in specified currency</returns>
    public Task<Money> ConvertToAsync(Money money, Currency toCurrency);

    /// <summary>
    /// Converts money to specified currency approximately based on static currency rates
    /// </summary>
    /// <param name="money"></param>
    /// <param name="toCurrency"></param>
    /// <returns>Approximate converted money</returns>
    public Money ApproximateTo(Money money, Currency toCurrency);
}