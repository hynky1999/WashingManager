using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PrackyASusarny.Data;
using PrackyASusarny.Data.EFCoreServices;

// ReSharper disable InconsistentNaming

namespace PrackyASusarny.Utils;

public static class EFExtensions
{
    public static IQueryable<T> MakeEager<T>(this IQueryable<T> query, IEntityType entity, string? parent = null)
        where T : class
    {
        var navigations = entity
            .GetDerivedTypesInclusive()
            .SelectMany(type => type.GetNavigations())
            .Distinct();

        foreach (var property in navigations)
        {
            string newParent = parent == null ? property.Name : $"{parent}.{property.Name}";
            query = query.Include(newParent);
            query = query.MakeEager(property.TargetEntityType, newParent);
        }

        return query;
    }

    public static IQueryable<T> FilterWithExpressions<T>(this IQueryable<T> query, Expression<Func<T, bool>>[] filters)
        where T : class
    {
        foreach (var filter in filters) query = query.Where(filter);

        return query;
    }

    public static IQueryable<T> SortWithKeys<T, TKey>(this IQueryable<T> query, SortOption<T, TKey>[] sortKeys)
        where T : class
    {
        if (sortKeys.Length == 0) return query;

        var firstSortKey = sortKeys[0];
        var querySort = firstSortKey.Ascending
            ? query.OrderBy(firstSortKey.KeyAccessor)
            : query.OrderByDescending(firstSortKey.KeyAccessor);
        foreach (var filter in sortKeys.Skip(1))
            querySort = filter.Ascending
                ? querySort.ThenBy(filter.KeyAccessor)
                : querySort.ThenByDescending(filter.KeyAccessor);

        return querySort;
    }

    public async static Task SaveChangeAsyncRethrow(this ApplicationDbContext context)
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException e)
        {
            throw new Errors.Folder.DbConcurrencyException(e.Message);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
        {
            throw new Errors.Folder.DbUpdateException(e.Message);
        }
    }
}