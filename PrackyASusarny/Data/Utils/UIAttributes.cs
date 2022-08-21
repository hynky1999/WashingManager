namespace PrackyASusarny.Data.Utils;

[AttributeUsage(AttributeTargets.Property)]
public class UIVisibility : Attribute
{
    public UIVisibility(UIVisibilityEnum visibility)
    {
        Visibility = visibility;
    }

    public UIVisibilityEnum Visibility { get; }
}

public enum UIVisibilityEnum
{
    Hidden,
    Disabled
}