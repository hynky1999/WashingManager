using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Castle.Core.Internal;

namespace App.Utils;

/// <summary>
/// Extension methods without a home
/// </summary>
public static class GenericUtils
{
    /// <summary>
    /// Awaits given task and write exception to console
    /// </summary>
    /// <param name="task">Task to be awaited</param>
    public static void FireAndForget(this Task task)
    {
        task.ContinueWith(t => { Console.WriteLine(t.Exception); },
            TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// Changes the type of the object to the given type
    /// Works with nullable value/ref types
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">throws if unable to detect type to convert to</exception>
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

    /// <summary>
    /// Gets the display name of type certain type based on the DisplayNameAttribute
    /// </summary>
    /// <param name="type"></param>
    /// <returns>display name</returns>
    public static string DisplayName(this Type type)
    {
        return type.GetAttribute<DisplayNameAttribute>()?.DisplayName ??
               type.Name;
    }

    /// <summary>
    /// Gets the display property based on DisplayAttribute/DisplayNameAttribute
    /// </summary>
    /// <param name="property"></param>
    /// <returns>Display Name</returns>
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