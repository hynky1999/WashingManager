using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace App.Utils;

/// <summary>
/// This class provides a helper method for working with Reflection.
/// Especially useful for Blazor to be able to bind a ValueChanged, Value and ValueExpression to a component on runtime.
/// </summary>
public static class ReflectionExtensions
{
    // Initialize EventCallbackGenericMethod
    private static readonly MethodInfo EventCallbackGenericMethod;

    static ReflectionExtensions()
    {
        EventCallbackGenericMethod = GetGenericEventCallback();
    }

    private static MethodInfo GetGenericEventCallback()
    {
        return typeof(EventCallbackFactory).GetMethods().Single(m =>
        {
            // Source https://github.com/meziantou/Meziantou.Framework/blob/ee664b6cf25ab0ae70ceaee55fcd3ef77c30dc4d/src/Meziantou.AspNetCore.Components/GenericFormField.cs
            if (m.Name != "Create" || !m.IsPublic || m.IsStatic ||
                !m.IsGenericMethod)
                return false;

            var generic = m.GetGenericArguments();
            if (generic.Length != 1)
                return false;

            var args = m.GetParameters();
            return args.Length == 2 &&
                   args[0].ParameterType == typeof(object) &&
                   args[1].ParameterType.IsGenericType &&
                   args[1].ParameterType.GetGenericTypeDefinition() ==
                   typeof(Action<>);
        });
    }

    /// <summary>
    /// Creates an Expression of member access to the given property of model of type T.
    /// </summary>
    /// <param name="propertyInfo">propertyInfo of property</param>
    /// <typeparam name="T">Type of model owning the property</typeparam>
    /// <typeparam name="TK">property conversion type</typeparam>
    /// <returns>Expression of member access</returns>
    public static Expression<Func<T, TK>> GetConcretePropertyExpression<T, TK>(
        this PropertyInfo propertyInfo)
    {
        var modelExprParam = Expression.Parameter(typeof(T));
        var property = Expression.Property(modelExprParam, propertyInfo);
        return Expression.Lambda<Func<T, TK>>(
            Expression.Convert(property, typeof(TK)), modelExprParam);
    }

    /// <summary>
    /// Creates an Property Access Expressions of the given property of model.
    /// Useful for Blazor to bind a ValueExpression
    /// </summary>
    /// <param name="model">model which owns the property</param>
    /// <param name="propertyInfo">property info of accessed property</param>
    /// <param name="castType">Type to cast property after access</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Property Access Expression of model</returns>
    public static LambdaExpression GetPropertyExpression<T>(this T model,
        PropertyInfo propertyInfo,
        Type? castType = null)
    {
        // (model) => model.Property
        var modelExpr = Expression.Constant(model);
        var property = Expression.Property(modelExpr, propertyInfo);
        if (castType != null)
        {
            var casted = Expression.Convert(property, castType);
            return Expression.Lambda(
                typeof(Func<>).MakeGenericType(propertyInfo.PropertyType),
                casted);
        }

        return Expression.Lambda(
            typeof(Func<>).MakeGenericType(propertyInfo.PropertyType),
            property);
    }

    /// <summary>
    /// Creates an EventCallback which Set given property of model to the callback value.
    /// Useful for Blazor to bind a ValueChanged
    /// </summary>
    /// <param name="model">model owning the property</param>
    /// <param name="receiver">receiver class</param>
    /// <param name="propertyInfo">property info a property set</param>
    /// <param name="parameterType">parameter type of EventCallback</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>EventCallback which sets given property to called value</returns>
    public static object? GetSetPropertyEventCallback<T>(this T model,
        object receiver, PropertyInfo propertyInfo,
        Type? parameterType = null)
    {
        var parameterTypeNotNull = parameterType ?? propertyInfo.PropertyType;
        // EventCallback<P>(receiver, (value) => model.Property = value)
        var propertyAccess = model.GetPropertyExpression(propertyInfo);
        var param = Expression.Parameter(parameterTypeNotNull, "value");
        BinaryExpression body;
        body = Expression.Assign(propertyAccess.Body,
            Expression.Convert(param, propertyInfo.PropertyType));
        var lamda = Expression.Lambda(
            typeof(Action<>).MakeGenericType(parameterTypeNotNull), body,
            param);
        return EventCallbackGenericMethod
            .MakeGenericMethod(parameterTypeNotNull)
            .Invoke(EventCallback.Factory, new[] {receiver, lamda.Compile()});
    }
}