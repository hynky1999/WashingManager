namespace App.Auth.Utils;

/// <summary>
/// Helper class for defining policies
/// We use roles for authorization of pages
/// </summary>
public static class Policies
{
    /// <summary>
    ///  Policy for <see cref="Claims.ManageUsers"/>
    /// </summary>
    public const string UserManagement = "UserManagement";

    /// <summary>
    ///  Policy for <see cref="Claims.ManageModels"/>
    /// </summary>
    public const string ModelManagement = "ModelManagement";

    /// <summary>
    ///  Policy for <see cref="Claims.ManageBorrows"/>
    /// </summary>
    public const string BorrowManagement = "BorrowManagement";
}