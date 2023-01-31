using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.EFCoreServices;

public class CurrencyService : ICurrencyService
{
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

    public Task<Money> ConvertToAsync(Money money, Currency toCurrency)
    {
        return Task.FromResult(ApproximateTo(money, toCurrency));
    }
}