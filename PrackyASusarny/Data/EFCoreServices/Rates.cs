namespace PrackyASusarny.Data.EFCoreServices;

public static class Rates
{
    public static int WMpricePerHalfHour = 6;
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