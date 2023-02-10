namespace App.Data.Utils;

/// <summary>
/// Represents status of Borrowable entity
/// </summary>
public enum Status
{
    /// <summary>
    /// Taken by someone
    /// It should have borrow with no end
    /// </summary>
    Taken,

    /// <summary>
    /// Broken and not usable
    /// </summary>
    Broken,

    /// <summary>
    /// Available for borrowing
    /// </summary>
    Free
}