using PrackyASusarny.Data.Models;

namespace PrackyASusarny.AdminPages;

public static class AdminConfig
{
    public static readonly Dictionary<string, Type> ModelNameToType = new Dictionary<string, Type>()
    {
        {nameof(WashingMachine), typeof(WashingMachine)},
        {nameof(Manual), typeof(Manual)}
    };
}