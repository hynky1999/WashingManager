using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace PrackyASusarny.Shared.Components
{
    public sealed class GenericFormField<TModel>
    {
        private static readonly MethodInfo s_eventCallbackFactoryCreate = GetEventCallbackFactoryCreate();

        private RenderFragment? _editorTemplate;
        private RenderFragment? _fieldValidationTemplate;
        public TModel Owner;

        private GenericFormField(PropertyInfo propertyInfo)
        {
            Property = propertyInfo;
        }

        public PropertyInfo Property { get; }

        public string DisplayName
        {
            get
            {
                var displayAttribute = Property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    var displayName = displayAttribute.GetName();
                    if (!string.IsNullOrEmpty(displayName))
                        return displayName;
                }

                var displayNameAttribute = Property.GetCustomAttribute<DisplayNameAttribute>();
                if (displayNameAttribute != null)
                {
                    var displayName = displayNameAttribute.DisplayName;
                    if (!string.IsNullOrEmpty(displayName))
                        return displayName;
                }

                return Property.Name;
            }
        }

        public string? Description
        {
            get
            {
                var displayAttribute = Property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    var description = displayAttribute.GetDescription();
                    if (!string.IsNullOrEmpty(description))
                        return description;
                }

                var descriptionAttribute = Property.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    var description = descriptionAttribute.Description;
                    if (!string.IsNullOrEmpty(description))
                        return description;
                }

                return null;
            }
        }

        public Type PropertyType => Property.PropertyType;

        public object Value
        {
            get => Property.GetValue(Owner);
            set
            {
                if (Property.SetMethod != null && !Equals(Value, value))
                {
                    Property.SetValue(Owner, value);
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public RenderFragment EditorTemplate
        {
            get
            {
                if (_editorTemplate != null)
                    return _editorTemplate;

                // () => Owner.Property
                var access = Expression.Property(Expression.Constant(Owner, typeof(TModel)), Property);
                var lambda = Expression.Lambda(typeof(Func<>).MakeGenericType(PropertyType), access);

                // Create(object receiver, Action<object> callback
                var method = s_eventCallbackFactoryCreate.MakeGenericMethod(PropertyType);

                // value => Field.Value = value;
                var changeHandlerParameter = Expression.Parameter(PropertyType);
                var body = Expression.Assign(Expression.Property(Expression.Constant(this), nameof(Value)),
                    Expression.Convert(changeHandlerParameter, typeof(object)));
                var changeHandlerLambda = Expression.Lambda(typeof(Action<>).MakeGenericType(PropertyType), body,
                    changeHandlerParameter);
                var changeHandler = method.Invoke(EventCallback.Factory,
                    new object[] {this, changeHandlerLambda.Compile()});

                return _editorTemplate ??= builder =>
                {
                    var (componentType, additonalAttributes) = GetEditorType(Property);
                    builder.OpenComponent(0, componentType);
                    builder.AddAttribute(1, "Value", Value);
                    builder.AddAttribute(2, "ValueChanged", changeHandler);
                    builder.AddAttribute(3, "ValueExpression", lambda);
                    builder.AddAttribute(4, "id", EditorId);
                    builder.AddMultipleAttributes(6, additonalAttributes);
                    builder.CloseComponent();
                };
            }
        }

        public RenderFragment? FieldValidationTemplate
        {
            get
            {
                if (!_form.EnableFieldValidation)
                    return null;

                return _fieldValidationTemplate ??= builder =>
                {
                    // () => Owner.Property
                    var access = Expression.Property(Expression.Constant(Owner, typeof(TModel)), Property);
                    var lambda = Expression.Lambda(typeof(Func<>).MakeGenericType(PropertyType), access);

                    builder.OpenComponent(0, typeof(ValidationMessage<>).MakeGenericType(PropertyType));
                    builder.AddAttribute(1, "For", lambda);
                    builder.CloseComponent();
                };
            }
        }

        public event EventHandler? ValueChanged;

        private static (Type ComponentType, IEnumerable<KeyValuePair<string, object>>? AdditonalAttributes)
            GetEditorType(PropertyInfo property)
        {
            var editorAttributes = property.GetCustomAttributes<EditorAttribute>();
            foreach (var editorAttribute in editorAttributes)
            {
                if (editorAttribute.EditorBaseTypeName == typeof(InputBase<>).AssemblyQualifiedName)
                    return (Type.GetType(editorAttribute.EditorTypeName, throwOnError: true)!, null);
            }

            if (property.PropertyType == typeof(bool))
                return (typeof(AntDesign.Checkbox), null);

            if (property.PropertyType == typeof(string))
            {
                return (typeof(InputText), null);
            }

            if (property.PropertyType == typeof(short))
                return (typeof(AntDesign.InputNumber<short>), null);

            if (property.PropertyType == typeof(int))
                return (typeof(AntDesign.InputNumber<int>), null);

            if (property.PropertyType == typeof(long))
                return (typeof(AntDesign.InputNumber<long>), null);

            if (property.PropertyType == typeof(float))
                return (typeof(AntDesign.InputNumber<float>), null);

            if (property.PropertyType == typeof(double))
                return (typeof(AntDesign.InputNumber<double>), null);

            if (property.PropertyType == typeof(decimal))
                return (typeof(AntDesign.InputNumber<decimal>), null);

            if (property.PropertyType.IsSubclassOf(ICRUDModel))
            {
                return (typeof(AntDesign.DatePicker<DateTime>), null);
            }

            if (property.PropertyType.IsEnum)
            {
                if (!property.PropertyType.IsDefined(typeof(FlagsAttribute), inherit: true))
                    return (typeof(AntDesign.EnumSelect<>).MakeGenericType(property.PropertyType), null);
            }

            if (property.PropertyType.)

                return (typeof(InputText), null);
        }

        private static MethodInfo GetEventCallbackFactoryCreate()
        {
            return typeof(EventCallbackFactory).GetMethods()
                .Single(m =>
                {
                    if (m.Name != "Create" || !m.IsPublic || m.IsStatic || !m.IsGenericMethod)
                        return false;

                    var generic = m.GetGenericArguments();
                    if (generic.Length != 1)
                        return false;

                    var args = m.GetParameters();
                    return args.Length == 2 && args[0].ParameterType == typeof(object) &&
                           args[1].ParameterType.IsGenericType &&
                           args[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>);
                });
        }
    }
}