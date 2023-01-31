namespace PrackyASusarny.Data.Constants;

public interface IRates
{
    public int WMpricePerHalfHour { get; }

    public int FlatBorrowPrice { get; }
    public int WMpricePerOverRes { get; }
    public int WMNoBorrowPenalty { get; }
    public Currency DBCurrency { get; }
}

public class Rates : IRates
{
    public int WMpricePerHalfHour => 6;
    public int FlatBorrowPrice => 10;
    public int WMpricePerOverRes => 30;
    public int WMNoBorrowPenalty => 50;

    public Currency DBCurrency => Currency.CZK;
}

public enum Currency
{
    CZK
}

public record Money
{
    public double Amount { get; set; }
    public Currency Currency { get; set; }

    public override string ToString()
    {
        return $"{Amount} {Currency}";
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new ArgumentException("Currencies must match");
        return new Money {Amount = a.Amount + b.Amount, Currency = a.Currency};
    }

    public static Money operator -(Money a)
    {
        return new Money {Amount = a.Amount, Currency = a.Currency};
    }
}