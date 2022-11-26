using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Data.ModelInterfaces;
// Todo add more key options

public interface IDBModel
{
    public string HumanReadableLoc(ILocalizationService loc);
}