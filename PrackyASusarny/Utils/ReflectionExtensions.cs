using System.Linq.Expressions;
using System.Reflection;

namespace PrackyASusarny.Utils;

public static class ReflectionExtensions
{
    public static Func<T, P> GetGetMethod<T, P>(Type ownerType, PropertyInfo propertyInfo)
    {
        var parameter = Expression.Parameter(ownerType, "context");
        var property = Expression.Property(parameter, propertyInfo);
        var lambda = Expression.Lambda(typeof(Func<,>).MakeGenericType(ownerType, propertyInfo.PropertyType), property,
            parameter);
        var xd = Expression.Lambda<Func<T, P>>(property, parameter).Compile();
        return xd;
    }
}