using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Pages.Features.AdminPages;

public static class AdminConfig
{
    public static readonly Dictionary<string, Type> ModelNameToType = new()
    {
        {nameof(WashingMachine), typeof(WashingMachine)},
        {nameof(Manual), typeof(Manual)},
        {nameof(Borrow), typeof(Borrow)}
    };
}