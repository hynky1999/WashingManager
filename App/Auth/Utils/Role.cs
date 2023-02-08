using Microsoft.AspNetCore.Identity;

namespace App.Auth.Utils;

/// <summary>
/// Implementation of <see cref="IdentityRole{TKey}"/>
/// We don't really had to implement this class, but it's good to have it for future use
/// </summary>
public class Role : IdentityRole<int>
{
    /// <inheritdoc />
    public Role(string role) : base(role)
    {
    }

    /// <inheritdoc />
    public Role()
    {
    }
}