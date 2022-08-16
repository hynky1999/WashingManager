using System.Reflection;

namespace PrackyASusarny.Data.ModelInterfaces;
// Todo add more key options

public interface DBModel
{
}

public interface IKeyable
{
    public int GetKey();
    public bool GetIsKeyProperty(PropertyInfo propertyInfo);
}