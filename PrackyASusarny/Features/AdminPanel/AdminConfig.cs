using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Features.AdminPanel;

public static class AdminConfig
{
    public static readonly Type[] AllowedTypes =
    {
        typeof(WashingMachine), typeof(BorrowPerson), typeof(Location),
        typeof(Manual), typeof(DryingRoom)
    };
}