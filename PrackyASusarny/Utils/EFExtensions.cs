using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PrackyASusarny.Data.EFCoreServices;

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

    public static IQueryable<T> FilterWithExpressions<T>(this IQueryable<T> query, Expression<Func<T, bool>>[] filters)
        where T : class
    {
        foreach (var filter in filters)
        {
            query = query.Where(filter);
        }

        return query;
    }

    public static IQueryable<T> SortWithKeys<T, TKey>(this IQueryable<T> query, SortOption<T, TKey>[] sortKeys)
        where T : class
    {
        if (sortKeys.Length == 0)
        {
            return query;
        }

        var firstSortKey = sortKeys[0];
        var querySort = firstSortKey.Ascending
            ? query.OrderBy(firstSortKey.KeyAccessor)
            : query.OrderByDescending(firstSortKey.KeyAccessor);
        foreach (var filter in sortKeys.Skip(1))
        {
            querySort = filter.Ascending
                ? querySort.ThenBy(filter.KeyAccessor)
                : querySort.ThenByDescending(filter.KeyAccessor);
        }

        return querySort;
    }
}