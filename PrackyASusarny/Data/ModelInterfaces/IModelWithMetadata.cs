using System.Reflection;

namespace PrackyASusarny.Data.ModelInterfaces;
// Todo add more key options

public class DbModel
{
    // Terrible but I have no idea how to enfore static methods on 
    // classes implementing this interface. Since I want them to define
    // human readable name on themselves instead of running central dict.
    public static string HumanReadableName => "IDbModel";
}

public interface IKeyable
{
    public int GetKey();
    public bool GetIsKeyProperty(PropertyInfo propertyInfo);
}