using AntDesign;
using App.Data.ServiceInterfaces;
using App.Errors;

namespace App.Utils;

/// <summary>
/// Extension methods for <see cref="IMessageService"/>.
/// </summary>
public static class IMessageServiceExtensions
{
    /// <summary>
    /// Sends a message based on action result.
    /// If the action throws an exception, it will be handled and a message informing user about exception will be sent.
    /// If the actions is successful nothing will be sent.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="loc"></param>
    /// <param name="action">Action to act upon</param>
    /// <param name="showArgException">Should Argument Exception also be handled ?</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Returns result of the task if successful</returns>
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

    /// <summary>
    /// Same as <see cref="GenericOnDBError{T}"/> but without return value.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="loc"></param>
    /// <param name="action"></param>
    /// <param name="showArgException"></param>
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