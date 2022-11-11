using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Features.BEManagment;

public static class ManagementConfig
{
    public static Type[] AllowedEntities =
        {typeof(WashingMachine), typeof(DryingRoom)};
}