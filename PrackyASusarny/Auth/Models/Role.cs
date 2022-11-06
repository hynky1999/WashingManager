using Microsoft.AspNetCore.Identity;

namespace PrackyASusarny.Auth.Models;

// Might be useful for future use
public class Role : IdentityRole
{
    public Role(string role) : base(role)
    {
    }

    public Role()
    {
    }
}