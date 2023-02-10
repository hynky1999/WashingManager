using System.Linq.Expressions;

namespace App.Data.Utils;

/// <summary>
/// A class representing sorting options based on key and direction
/// </summary>
/// <typeparam name="T">Type of item to sort</typeparam>
/// <typeparam name="TKey">Type of key</typeparam>
public struct SortOption<T, TKey>
{
    /// <summary>
    /// Based on what key to sort ?
    /// </summary>
    public Expression<Func<T, TKey>> KeyAccessor { get; set; }

    /// <summary>
    /// Should sort in ascending order
    /// </summary>
    public bool Ascending { get; set; }
}