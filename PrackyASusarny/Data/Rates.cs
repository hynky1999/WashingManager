namespace PrackyASusarny.Data;

public static class Rates
{
    public const int WMpricePerHalfHour = 6;
}

public struct Price
{
    public int Amount { get; set; }
    public string Currency { get; set; }

    public override string ToString()
    {
        return $"{Amount} {Currency}";
    }
}

public static class ReservationConstant
{
    public static Duration MaxReservationHours = Duration.FromHours(24 * 4);
    public static Duration MinReservationHours = Duration.FromHours(1);
    public static Duration MinHoursBeforeReservation = Duration.FromHours(2);
    public static Duration MinReservationCancelHours = Duration.FromHours(24);
}