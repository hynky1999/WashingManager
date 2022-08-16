using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Data.Utils;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Shared.Components.GenericForm;

public sealed class GenericInput<TModel>
{
    private readonly TModel _owner;
    private readonly PropertyInfo _propertyInfo;
    private readonly IServiceProvider _serviceProvider;

    private RenderFragment? _fieldFragment;
    public EventCallback ValueChanged;

    public GenericInput(TModel owner, PropertyInfo propertyInfo, IServiceProvider serviceProvider)
    {
        _owner = owner;
        _propertyInfo = propertyInfo;
        _serviceProvider = serviceProvider;
    }

    public object? Value
    {
        get => _propertyInfo.GetValue(_owner);
        set
        {
            if (_propertyInfo.SetMethod != null && !Equals(Value, value))
            {
                _propertyInfo.SetValue(_owner, value);
                ValueChanged.InvokeAsync();
            }
        }
    }

    public RenderFragment FieldFragment
    {
        get
        {
            var visibilityAttr = _propertyInfo.GetCustomAttribute<UIVisibility>();

            if (_fieldFragment != null)
            {
                return _fieldFragment;
            }

            var propertyExpression = _owner.GetPropertyExpression(_propertyInfo);
            var valueChangedCallback = _owner.GetSetPropertyEventCallback(this, _propertyInfo);


            return _fieldFragment ??= builder =>
            {
                var (componentType, additonalAttributes) = GetFieldType(_propertyInfo, _serviceProvider);
                builder.OpenComponent(0, componentType);
                builder.AddAttribute(1, "Value", Value);
                builder.AddAttribute(2, "ValueChanged", valueChangedCallback);
                builder.AddAttribute(3, "ValueExpression", propertyExpression);
                builder.AddAttribute(4, "Disabled", visibilityAttr?.Visibility == UIVisibilityEnum.Disabled);
                builder.AddMultipleAttributes(6, additonalAttributes);
                builder.CloseComponent();
            };
        }
    }

    public string? Description
    {
        get
        {
            var displayAttribute = _propertyInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
            {
                var description = displayAttribute.GetDescription();
                if (!string.IsNullOrEmpty(description))
                    return description;
            }

            var descriptionAttribute = _propertyInfo.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
            {
                var description = descriptionAttribute.Description;
                if (!string.IsNullOrEmpty(description))
                    return description;
            }

            return null;
        }
    }

    public string DisplayName
    {
        get
        {
            var displayAttribute = _propertyInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
            {
                var displayName = displayAttribute.GetName();
                if (!string.IsNullOrEmpty(displayName))
                    return displayName;
            }

            var displayNameAttribute = _propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
            if (displayNameAttribute != null)
            {
                var displayName = displayNameAttribute.DisplayName;
                if (!string.IsNullOrEmpty(displayName))
                    return displayName;
            }

            return _propertyInfo.Name;
        }
    }


    private static (Type ComponentType, IEnumerable<KeyValuePair<string, object>>? AdditonalAttributes) GetFieldType(
        PropertyInfo propertyInfo, IServiceProvider sp)
    {
        var editorAttributes = propertyInfo.GetCustomAttributes<EditorAttribute>();
        foreach (var editorAttribute in editorAttributes)
        {
            if (editorAttribute.EditorBaseTypeName == typeof(InputBase<>).AssemblyQualifiedName)
                return (Type.GetType(editorAttribute.EditorTypeName, throwOnError: true)!, null);
        }

        if (propertyInfo.PropertyType == typeof(bool))
            return (typeof(AntDesign.Checkbox), null);

        if (propertyInfo.PropertyType == typeof(string))
        {
            return (typeof(AntDesign.Input<string>), null);
        }

        if (propertyInfo.PropertyType == typeof(short))
            return (typeof(AntDesign.InputNumber<short>), null);

        if (propertyInfo.PropertyType == typeof(int))
            return (typeof(AntDesign.InputNumber<int>), null);

        if (propertyInfo.PropertyType == typeof(uint))
            return (typeof(AntDesign.InputNumber<uint>), null);

        if (propertyInfo.PropertyType == typeof(long))
            return (typeof(AntDesign.InputNumber<long>), null);

        if (propertyInfo.PropertyType == typeof(float))
            return (typeof(AntDesign.InputNumber<float>), null);

        if (propertyInfo.PropertyType == typeof(double))
            return (typeof(AntDesign.InputNumber<double>), null);

        if (propertyInfo.PropertyType == typeof(decimal))
            return (typeof(AntDesign.InputNumber<decimal>), null);

        if (propertyInfo.PropertyType == typeof(DateTime))
        {
            return (typeof(AntDesign.DatePicker<DateTime>), null);
        }

        if (propertyInfo.PropertyType.IsEnum)
        {
            return (typeof(AntDesign.EnumSelect<>).MakeGenericType(propertyInfo.PropertyType), null);
        }

        if (sp.GetService(typeof(ICrudService<>).MakeGenericType(propertyInfo.PropertyType)) is not null)
        {
            return (typeof(ModelSelect<>).MakeGenericType(propertyInfo.PropertyType), null);
        }

        throw new NotSupportedException($"The type {propertyInfo.PropertyType} is not supported.");
    }

    internal static List<GenericInput<TModel>> Create(TModel model, IServiceProvider serviceProvider)
    {
        var result = new List<GenericInput<TModel>>();
        var properties =
            typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        foreach (var prop in properties)
        {
            // Skip readonly properties
            if (prop.SetMethod == null)
                continue;

            if (prop.GetCustomAttribute<EditableAttribute>() is { } editor && !editor.AllowEdit)
                continue;
            if (prop.GetCustomAttribute<UIVisibility>() is {Visibility: UIVisibilityEnum.Hidden})
            {
                continue;
            }

            var field = new GenericInput<TModel>(model, prop, serviceProvider);
            result.Add(field);
        }

        return result;
    }
}