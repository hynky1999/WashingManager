using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AntDesign;
using App.Data.ServiceInterfaces;
using App.Data.Utils;
using App.Shared.Components.NodaComponents;
using App.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace App.Shared.GenericComponenets.GenericForm;
// Source https://github.com/meziantou/Meziantou.Framework/blob/ee664b6cf25ab0ae70ceaee55fcd3ef77c30dc4d/src/Meziantou.AspNetCore.Components/GenericFormField.cs

/// <summary>
/// Class which represents a generic field in a form for a property of a given type.
/// It allows to create a generic field with a label and description
/// The component is chosen based on the type of the field
/// for more <see cref="GetFieldType"/>
/// </summary>
/// <typeparam name="TModel">Model for which the form is created</typeparam>
public sealed class GenericInput<TModel>
{
    private readonly TModel _owner;
    private readonly PropertyInfo _propertyInfo;
    private readonly IServiceProvider _serviceProvider;

    private readonly PropertyInfo _valuePropertyInfo =
        typeof(GenericInput<TModel>).GetProperty(nameof(Value))!;

    private RenderFragment? _fieldFragment;

    /// <summary>
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="propertyInfo"></param>
    /// <param name="serviceProvider"></param>
    public GenericInput(TModel owner, PropertyInfo propertyInfo,
        IServiceProvider serviceProvider)
    {
        _owner = owner;
        _propertyInfo = propertyInfo;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Notifies the form that the value of the field has changed
    /// </summary>
    public EventHandler? ValueChanged { get; set; }

    /// <summary>
    /// Value of the field
    /// </summary>
    public object? Value
    {
        get => _propertyInfo.GetValue(_owner);
        set
        {
            _propertyInfo.SetValue(_owner, value);
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Fragment that can be used to render the field
    /// </summary>
    public RenderFragment FieldFragment
    {
        get
        {
            var visibilityAttr =
                _propertyInfo.GetCustomAttribute<UIVisibility>();

            if (_fieldFragment != null) return _fieldFragment;

            var propertyExpression =
                _owner.GetPropertyExpression(_propertyInfo);
            var valueChangedCallback =
                this.GetSetPropertyEventCallback(this, _valuePropertyInfo,
                    _propertyInfo.PropertyType);


            return _fieldFragment ??= builder =>
            {
                var (componentType, additonalAttributes) =
                    GetFieldType(_propertyInfo, _serviceProvider);
                builder.OpenComponent(0, componentType);
                builder.AddAttribute(1, "Value", Value);
                builder.AddAttribute(2, "ValueChanged", valueChangedCallback);
                builder.AddAttribute(3, "ValueExpression", propertyExpression);
                if (visibilityAttr?.Visibility != null)
                    builder.AddAttribute(4, "Disabled", true);

                builder.AddMultipleAttributes(5, additonalAttributes);
                builder.CloseComponent();
            };
        }
    }

    /// <summary>
    /// Description of the field based on Attributes
    /// </summary>
    public string? Description
    {
        get
        {
            var displayAttribute =
                _propertyInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
            {
                var description = displayAttribute.GetDescription();
                if (!string.IsNullOrEmpty(description))
                    return description;
            }

            var descriptionAttribute =
                _propertyInfo.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
            {
                var description = descriptionAttribute.Description;
                if (!string.IsNullOrEmpty(description))
                    return description;
            }

            return null;
        }
    }

    /// <summary>
    /// Label of the field based on Attributes
    /// </summary>
    public string DisplayName => _propertyInfo.DisplayName();


    private static (Type ComponentType,
        IEnumerable<KeyValuePair<string, object>>? AdditonalAttributes)
        GetFieldType(
            PropertyInfo propertyInfo, IServiceProvider sp)
    {
        var editorAttributes =
            propertyInfo.GetCustomAttributes<EditorAttribute>();
        foreach (var editorAttribute in editorAttributes)
            if (editorAttribute.EditorBaseTypeName ==
                typeof(InputBase<>).AssemblyQualifiedName)
                return (Type.GetType(editorAttribute.EditorTypeName, true)!,
                    null);

        var realType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
        realType ??= propertyInfo.PropertyType;

        if (realType == typeof(bool))
            return (typeof(Checkbox), null);

        if (realType == typeof(string))
            return (typeof(Input<>).MakeGenericType(propertyInfo.PropertyType),
                null);
        if (realType == typeof(char))
        {
            var uppers = Enumerable.Range('A', 'Z' - 'A' + 1)
                .Select(x => (char) x);
            var lowers = Enumerable.Range('a', 'z' - 'a' + 1)
                .Select(x => (char) x);

            char[] allowedChars = uppers.Concat(lowers).ToArray();
            return (
                typeof(CharInput<>).MakeGenericType(propertyInfo.PropertyType),
                new List<KeyValuePair<string, object>>()
                {
                    new("AllowedChars", allowedChars)
                });
        }

        if (realType == typeof(short) || realType == typeof(uint) ||
            realType == typeof(int) || realType == typeof(long) ||
            realType == typeof(float) || realType == typeof(double) ||
            realType == typeof(decimal))
            return (
                typeof(AntDesign.InputNumber<>).MakeGenericType(propertyInfo
                    .PropertyType), null);

        if (realType == typeof(Instant))
        {
            var attrs = new List<KeyValuePair<string, object>>();
            var dateTimeType =
                propertyInfo.GetCustomAttribute<DataTypeAttribute>();
            if (dateTimeType is null ||
                dateTimeType.DataType == DataType.DateTime)
                attrs.Add(new KeyValuePair<string, object>("showTime", true));

            // Nullable
            if (realType != propertyInfo.PropertyType)
                return (typeof(InstantPickerNullable), attrs);

            return (typeof(InstantPicker), attrs);
        }


        if (realType.IsEnum)
            return (
                typeof(EnumSelect<>).MakeGenericType(propertyInfo.PropertyType),
                null);

        if (sp.GetService(typeof(ICrudService<>).MakeGenericType(realType)) is
            not null)
            return (
                typeof(ModelSelect<>).MakeGenericType(propertyInfo
                    .PropertyType), new List<KeyValuePair<string, object>>()
                {
                    new("CrudInitialize", true)
                });


        throw new NotSupportedException(
            $"The type {propertyInfo.PropertyType} is not supported.");
    }

    internal static List<GenericInput<TModel>> Create(TModel model,
        IServiceProvider serviceProvider)
    {
        var result = new List<GenericInput<TModel>>();
        var properties =
            typeof(TModel).GetProperties(BindingFlags.Public |
                                         BindingFlags.Instance |
                                         BindingFlags.FlattenHierarchy);
        foreach (var prop in properties)
        {
            // Skip readonly properties
            if (prop.SetMethod == null)
                continue;

            if (prop.GetCustomAttribute<EditableAttribute>() is { } editor &&
                !editor.AllowEdit)
                continue;
            if (prop.GetCustomAttribute<UIVisibility>() is
                {Visibility: UIVisibilityEnum.Hidden}) continue;

            var field = new GenericInput<TModel>(model, prop, serviceProvider);
            result.Add(field);
        }

        return result;
    }
}