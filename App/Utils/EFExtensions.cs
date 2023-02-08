using System.Linq.Expressions;
using App.Data.Utils;
using App.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DbUpdateException = Microsoft.EntityFrameworkCore.DbUpdateException;

// ReSharper disable InconsistentNaming

namespace App.Utils;

/// <summary>
/// Extension methods for EF Core.
/// </summary>
public static class EFExtensions
{
    /// <summary>
    /// Makes the query to take all related entities of passed entity.
    /// Will not work if there is a circular reference !
    /// </summary>
    /// <param name="query">Query </param>
    /// <param name="entity">Entity to get eager</param>
    /// <param name="parent">parent of entity should be null</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Eager query</returns>
    public static IQueryable<T> MakeEager<T>(this IQueryable<T> query,
        IEntityType entity, string? parent = null)
        where T : class
    {
        var navigations = entity
            .GetDerivedTypesInclusive()
            .SelectMany(type => type.GetNavigations())
            .Distinct();

        foreach (var property in navigations)
        {
            var newParent = parent == null
                ? property.Name
                : $"{parent}.{property.Name}";
            query = query.Include(newParent);
            query = query.MakeEager(property.TargetEntityType, newParent);
        }

        return query;
    }

    /// <summary>
    /// Filters the query based on the passed filter.
    /// </summary>
    /// <param name="query">Query to filter</param>
    /// <param name="filters">Function to filter by</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Filtered query</returns>
    public static IQueryable<T> FilterWithExpressions<T>(
        this IQueryable<T> query, Expression<Func<T, bool>>[] filters)
        where T : class
    {
        foreach (var filter in filters) query = query.Where(filter);

        return query;
    }

    /// <summary>
    /// Sorts the query based on the passed sorters.
    /// </summary>
    /// <param name="query">Query to sort</param>
    /// <param name="sortKeys">Sort options</param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <returns>Sorted query</returns>
    public static IQueryable<T> SortWithKeys<T, TKey>(this IQueryable<T> query,
        SortOption<T, TKey>[] sortKeys)
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

    /// <summary>
    /// Save changes and rethrows the exceptions that are compatible with frontend.
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="DbConcurrencyException"></exception>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException"></exception>
    public static async Task SaveChangeAsyncRethrow(
        this DbContext context)
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new DbConcurrencyException(e.Message);
        }
        catch (DbUpdateException e)
        {
            throw new Errors.DbUpdateException(e.Message);
        }
    }

    /// <summary>
    /// Calls a method N times with delay between each call until not error thrown.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="n"></param>
    /// <param name="delay"></param>
    public static async Task TryNTimesAsync(Func<Task> action, int n = 10,
        int delay = 5000)
    {
        for (var i = 0; i < n; i++)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception)
            {
                if (i == n - 1) throw;
            }

            await Task.Delay(delay);
        }
    }
}