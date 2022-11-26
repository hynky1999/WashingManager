using AntDesign;
using PrackyASusarny.Errors.Folder;

public static class IMessageServiceExtensions
{
    public async static Task<T?> GenericOnDBError<T>(this IMessageService msg,
        Func<Task<T>> action, bool showArgException = true)
    {
        try
        {
            return await action();
        }
        catch (DbException ex)
        {
            msg.Error("Failed");
        }
        catch (ArgumentException ex)
        {
            if (showArgException)
                msg.Error(ex.Message);
        }
        catch (Exception ex)
        {
            msg.Error("Something went wrong");
        }

        return default;
    }

    public async static Task GenericOnDBError(this IMessageService msg,
        Func<Task> action, bool showArgException = true)
    {
        await msg.GenericOnDBError<bool>(async () =>
        {
            await action();
            return true;
        }, showArgException);
    }
}