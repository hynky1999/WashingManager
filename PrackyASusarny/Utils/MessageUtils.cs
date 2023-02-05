using AntDesign;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Errors.Folder;

namespace PrackyASusarny.Utils;

public static class IMessageServiceExtensions
{
    public static async Task<T?> GenericOnDBError<T>(this IMessageService msg,
        ILocalizationService loc, Func<Task<T>> action,
        bool showArgException = true)
    {
        try
        {
            return await action();
        }
        catch (DbException)
        {
            msg.Error(loc["Failed"]).FireAndForget();
        }
        catch (ArgumentException ex)
        {
            if (showArgException)
                msg.Error(loc[ex.Message]).FireAndForget();
        }
        catch (Exception)
        {
            msg.Error(loc["Something went wrong"]).FireAndForget();
        }

        return default;
    }

    public static async Task GenericOnDBError(this IMessageService msg,
        ILocalizationService loc,
        Func<Task> action, bool showArgException = true)
    {
        await msg.GenericOnDBError(loc, async () =>
        {
            await action();
            return true;
        }, showArgException);
    }
}