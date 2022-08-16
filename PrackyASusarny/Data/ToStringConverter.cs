using System.ComponentModel;
using System.Globalization;

namespace PrackyASusarny.Data;

class ToStringConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str)
        {
        }

        return base.ConvertFrom(context, culture, value);
    }
}