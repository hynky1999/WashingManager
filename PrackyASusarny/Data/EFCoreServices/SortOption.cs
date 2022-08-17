using System.Linq.Expressions;

namespace PrackyASusarny.Data.EFCoreServices;

public struct SortOption<T, TKey>
{
    public Expression<Func<T, TKey>> KeyAccessor { get; set; }
    public bool Ascending { get; set; }
}