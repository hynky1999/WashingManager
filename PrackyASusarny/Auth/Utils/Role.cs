using Microsoft.AspNetCore.Identity;

namespace PrackyASusarny.Auth.Utils;

// Might be useful for future use
public class Role : IdentityRole<int>
{
    public Role(string role) : base(role)
    {
    }

    public Role()
    {
    }
}