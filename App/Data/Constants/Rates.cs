namespace App.Data.Constants;

/// <summary>
/// Interface for Rates Constants
/// </summary>
public interface IRates
{
    /// <summary>
    /// Price per half hour of borrow
    /// </summary>
    public int PricePerHalfHour { get; }

    /// <summary>
    /// Flat price per start of borrow
    /// </summary>
    public int FlatBorrowPrice { get; }

    /// <summary>
    /// Price per Borrow over Reservation period for one <see cref="IReservationConstant.ReservationPostponeDur"/>
    /// </summary>
    public int PricePerOverRes { get; }

    /// <summary>
    /// Price for not picking up the borrow in reservation time
    /// </summary>
    public int NoBorrowPenalty { get; }

    /// <summary>
    /// Currency of DB
    /// </summary>
    public Currency DBCurrency { get; }
}

/// <summary>
/// Implementation of <see cref="IRates"/>
/// </summary>
public class Rates : IRates
{
    /// <summary>
    /// See <see cref="IRates.PricePerHalfHour"/>
    /// </summary>
    public int PricePerHalfHour => 6;

    /// <summary>
    /// See <see cref="IRates.FlatBorrowPrice"/>
    /// </summary>
    public int FlatBorrowPrice => 10;

    /// <summary>
    /// See <see cref="IRates.PricePerOverRes"/>
    /// </summary>
    public int PricePerOverRes => 30;

    /// <summary>
    /// See <see cref="IRates.NoBorrowPenalty"/>
    /// </summary>
    public int NoBorrowPenalty => 50;

    /// <summary>
    /// See <see cref="IRates.DBCurrency"/>
    /// </summary>
    public Currency DBCurrency => Currency.CZK;
}

/// <summary>
/// Enum for Currency
/// </summary>
public enum Currency
{
    /// <summary>
    /// Czech Koruna
    /// </summary>
    CZK
}

/// <summary>
/// Record representing Money
/// </summary>
public record Money
{
    /// <summary>
    /// Amount of money
    /// </summary>
    public double Amount { get; set; }

    /// <summary>
    /// Currency of money
    /// </summary>
    public Currency Currency { get; set; }

    /// <summary>
    /// Pretty print of Money
    /// </summary>
    /// <returns>ToString repr</returns>
    public override string ToString()
    {
        return $"{Amount} {Currency}";
    }

    /// <summary>
    /// Adds two Money
    /// </summary>
    /// <param name="a">Money 1</param>
    /// <param name="b">Money 2</param>
    /// <returns>Sum of Money</returns>
    /// <exception cref="ArgumentException">Throws if currency don't match</exception>
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new ArgumentException("Currencies must match");
        return a with {Amount = a.Amount + b.Amount};
    }

    /// <summary>
    /// Negates Money amount
    /// </summary>
    /// <param name="a">Money 1</param>
    /// <returns>Negated money</returns>
    public static Money operator -(Money a)
    {
        return new Money {Amount = a.Amount, Currency = a.Currency};
    }
}