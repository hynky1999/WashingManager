using App.Data.Constants;
using App.Data.ServiceInterfaces;

namespace App.Data.EFCoreServices;

/// <summary>
/// EF Core implementation of <see cref="ICurrencyService"/>.
/// </summary>
public class CurrencyService : ICurrencyService
{
    /// <inheritdoc />
    public Money ApproximateTo(Money money, Currency toCurrency)
    {
        Money convertedMoney;
        switch (toCurrency)
        {
            case Currency.CZK:
                convertedMoney = money with {Currency = Currency.CZK};
                break;
            default:
                throw new ArgumentException("Unsported currency");
        }

        return convertedMoney;
    }

    /// <inheritdoc />
    public Task<Money> ConvertToAsync(Money money, Currency toCurrency)
    {
        return Task.FromResult(ApproximateTo(money, toCurrency));
    }
}