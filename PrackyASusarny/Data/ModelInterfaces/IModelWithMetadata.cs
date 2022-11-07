namespace PrackyASusarny.Data.ModelInterfaces;
// Todo add more key options

public abstract class DbModel
{
    // Terrible but I have no idea how to enfore static methods on 
    // classes implementing this interface. Since I want them to define
    // human readable name on themselves instead of running central dict.
    public static string HumanReadableName => "IDbModel";
    public abstract string Label { get; }
}