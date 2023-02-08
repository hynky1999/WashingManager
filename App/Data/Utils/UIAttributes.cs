// ReSharper disable InconsistentNaming

namespace App.Data.Utils;

/// <summary>
/// A class representing Visibility of a UI element
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class UIVisibility : Attribute
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="visibility">should item be visible</param>
    public UIVisibility(UIVisibilityEnum visibility)
    {
        Visibility = visibility;
    }

    /// <summary>
    /// Should item be Visible ?
    /// </summary>
    public UIVisibilityEnum Visibility { get; }
}

/// <summary>
/// Type of visibility
/// </summary>
public enum UIVisibilityEnum
{
    /// <summary>
    /// Not visible
    /// </summary>
    Hidden,

    /// <summary>
    /// Disabled look
    /// </summary>
    Disabled
}