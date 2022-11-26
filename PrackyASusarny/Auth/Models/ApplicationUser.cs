using Microsoft.AspNetCore.Identity;
using PrackyASusarny.Data.ModelInterfaces;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Auth.Models;
# nullable disable

public sealed class ApplicationUser : IdentityUser<int>, IDBModel
{
    public ApplicationUser() : base()
    {
    }

    public ApplicationUser(string userName) : base(userName)
    {
    }


    [ProtectedPersonalData] public string Name { get; set; }

    [ProtectedPersonalData] public string Surname { get; set; }

    public string HumanReadableLoc(ILocalizationService loc)
    {
        return $"{Name} {Surname}";
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}