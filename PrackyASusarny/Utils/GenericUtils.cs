namespace PrackyASusarny.Utils;

public class GenericUtils
{
    public static T ChangeType<T>(object value)
    {
        var t = typeof(T);

        if (t.IsGenericType &&
            t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            if (value == null)
            {
                return default(T);
            }

            t = Nullable.GetUnderlyingType(t);
        }

        Console.WriteLine(t);
        Console.WriteLine(value.GetType());
        return (T) Convert.ChangeType(value, t);
    }
}