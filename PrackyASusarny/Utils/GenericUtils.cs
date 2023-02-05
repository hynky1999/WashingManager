using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Castle.Core.Internal;

namespace PrackyASusarny.Utils;

public static class GenericUtils
{
    public static void FireAndForget(this Task task)
    {
        task.ContinueWith(t => { Console.WriteLine(t.Exception); },
            TaskContinuationOptions.OnlyOnFaulted);
    }

    public static T? ChangeType<T>(object? value)
    {
        var t = typeof(T);

        if (t.IsGenericType &&
            t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (value == null)
            {
                return default;
            }

            t = Nullable.GetUnderlyingType(t);
        }

        if (t == null)
        {
            throw new ArgumentNullException(nameof(t));
        }


        var converted = Convert.ChangeType(value, t);
        if (converted == null)
        {
            return default;
        }

        return (T) converted;
    }

    public static string DisplayName(this Type type)
    {
        return type.GetAttribute<DisplayNameAttribute>()?.DisplayName ??
               type.Name;
    }

    public static string DisplayName(this PropertyInfo property)
    {
        var displayAttribute =
            property.GetCustomAttribute<DisplayAttribute>();
        if (displayAttribute != null)
        {
            var displayName = displayAttribute.GetName();
            if (!string.IsNullOrEmpty(displayName))
                return displayName;
        }

        var displayNameAttribute =
            property.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttribute != null)
        {
            var displayName = displayNameAttribute.DisplayName;
            if (!string.IsNullOrEmpty(displayName))
                return displayName;
        }

        return property.Name;
    }
}