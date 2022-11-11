using Microsoft.AspNetCore.Identity;
using PrackyASusarny.Data.ModelInterfaces;

namespace PrackyASusarny.Auth.Models;

public class User : IdentityUser, IDbModel, ICloneable
{
    public User()
    {
    }

    public User(string userName) : base(userName)
    {
    }

    public string HumanReadable => $"User ID: {Id}, User Name: {UserName}";

    public object Clone()
    {
        return MemberwiseClone();
    }
}