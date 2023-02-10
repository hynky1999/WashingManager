using App.Data.ModelInterfaces;
using App.Data.ServiceInterfaces;
using Microsoft.AspNetCore.Identity;

namespace App.Auth.Models;
# nullable disable

/// <summary>
/// Represents a user in the identity system.
/// </summary>
public sealed class ApplicationUser : IdentityUser<int>, IDBModel
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationUser()
    {
    }

    /// <summary>
    /// Constructor with username
    /// </summary>
    /// <param name="userName"></param>
    public ApplicationUser(string userName) : base(userName)
    {
    }


    /// <summary>
    /// Name of user
    /// </summary>
    [ProtectedPersonalData]
    public string Name { get; set; }

    /// <summary>
    /// Surname of user
    /// </summary>
    [ProtectedPersonalData]
    public string Surname { get; set; }

    /// <summary>
    /// Cash of User
    /// </summary>
    [ProtectedPersonalData]
    public double Cash { get; set; }


    /// <inheritdoc />
    public string HumanReadableLoc(ILocalizationService loc)
    {
        return $"{Name} {Surname}";
    }

    /// <summary>
    /// Memberwise clone
    /// </summary>
    /// <returns></returns>
    public object Clone()
    {
        return MemberwiseClone();
    }
}