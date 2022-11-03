using Microsoft.AspNetCore.Identity;

namespace PrackyASusarny.Auth.Models;

public class User : IdentityUser
{
    public User() : base()
    {
    }

    public User(string userName) : base(userName)
    {
    }
}