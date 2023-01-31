using PrackyASusarny.Data.Constants;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface ICurrencyService
{
    public Task<Money> ConvertToAsync(Money money, Currency toCurrency);
    public Money ApproximateTo(Money money, Currency toCurrency);
}