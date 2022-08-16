namespace PrackyASusarny.Data.Utils;

[System.AttributeUsage(System.AttributeTargets.Property)]
public class UIVisibility : System.Attribute
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