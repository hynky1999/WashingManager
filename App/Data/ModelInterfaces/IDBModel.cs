using App.Data.ServiceInterfaces;

namespace App.Data.ModelInterfaces;

/// <summary>
/// This interface allows DB Entities to be able to describe themselves.
/// Since the model description will differ only in the properties we also
/// pass a ILocalization service so that the description can be localized.
/// Otherwise it would be impossible to do so.
/// </summary>
public interface IDBModel
{
    /// <summary>
    /// Returns a description of the model.
    /// </summary>
    /// <param name="loc">
    /// The localization service to use for localization.
    /// </param>
    public string HumanReadableLoc(ILocalizationService loc);
}