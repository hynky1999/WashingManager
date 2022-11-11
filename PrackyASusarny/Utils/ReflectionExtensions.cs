using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace PrackyASusarny.Utils;

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

    public static Expression<Func<T, TK>> GetConcretePropertyExpression<T, TK>(
        this PropertyInfo propertyInfo)
    {
        var modelExprParam = Expression.Parameter(typeof(T));
        var property = Expression.Property(modelExprParam, propertyInfo);
        return Expression.Lambda<Func<T, TK>>(
            Expression.Convert(property, typeof(TK)), modelExprParam);
    }

    public static LambdaExpression GetPropertyExpression<T>(this T model,
        PropertyInfo propertyInfo,
        bool castToObject = false)
    {
        // (model) => model.Property
        var modelExpr = Expression.Constant(model);
        var property = Expression.Property(modelExpr, propertyInfo);
        if (castToObject)
        {
            var casted = Expression.Convert(property, typeof(object));
            return Expression.Lambda(
                typeof(Func<>).MakeGenericType(propertyInfo.PropertyType),
                casted);
        }

        return Expression.Lambda(
            typeof(Func<>).MakeGenericType(propertyInfo.PropertyType),
            property);
    }

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