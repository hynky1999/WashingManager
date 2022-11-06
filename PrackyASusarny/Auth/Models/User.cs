using Microsoft.AspNetCore.Identity;

namespace PrackyASusarny.Auth.Models;

public class User : IdentityUser
{
    public User()
    {
    }

    public User(string userName) : base(userName)
    {
    }
}