using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PrackyASusarny.Utils;

public static class EFExtensions
{
    public static IQueryable<T> MakeEager<T>(this IQueryable<T> query, IEntityType entity) where T : class
    {
        var navigations = entity
            .GetDerivedTypesInclusive()
            .SelectMany(type => type.GetNavigations())
            .Distinct();

        foreach (var property in navigations)
        {
            query = query.Include(property.Name);
        }

        return query;
    }
}