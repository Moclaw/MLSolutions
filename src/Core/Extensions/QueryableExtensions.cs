

using System.Collections.Immutable;
using System.Linq.Expressions;

namespace MLSolutions;

/// <summary>
/// Provides extension methods for IQueryable to support dynamic ordering and asynchronous operations.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Orders the elements of a sequence based on a specified property name and sort direction.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to order.</param>
    /// <param name="propertyName">The name of the property to order by.</param>
    /// <param name="isDescending">Indicates whether the sorting should be descending.</param>
    /// <returns>An <see cref="IQueryable{T}"/> whose elements are sorted according to the specified property.</returns>
    public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string propertyName, bool isDescending) => ApplyOrder(source, propertyName, isDescending);

    /// <summary>
    /// Applies ordering to the elements of a sequence based on a specified property name and sort direction.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to order.</param>
    /// <param name="propertyName">The name of the property to order by.</param>
    /// <param name="isDescending">Indicates whether the sorting should be descending.</param>
    /// <param name="isNextOrder">Indicates whether this is a subsequent ordering (e.g., ThenBy).</param>
    /// <returns>An <see cref="IQueryable{T}"/> whose elements are sorted according to the specified property.</returns>
    private static IQueryable<T> ApplyOrder<T>(IQueryable<T> source, string propertyName, bool isDescending, bool isNextOrder = false)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Order by property should not be empty", nameof(propertyName));
        }

        var parameter = Expression.Parameter(typeof(T), "p");

        Expression propertyAccess = parameter;
        var propertyParts = propertyName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < propertyParts.Length; i++)
        {
            propertyAccess = Expression.PropertyOrField(propertyAccess, propertyParts[i]);
        }

        var orderByExpression = Expression.Lambda(propertyAccess, parameter);
        var methodName = isNextOrder
            ? (isDescending ? "ThenByDescending" : "ThenBy")
            : (isDescending ? "OrderByDescending" : "OrderBy");

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), propertyAccess.Type],
            source.Expression,
            Expression.Quote(orderByExpression));

        return source.Provider.CreateQuery<T>(resultExpression);
    }

    /// <summary>
    /// Applies a subsequent ordering to the elements of a sequence based on a specified property name and sort direction.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to order.</param>
    /// <param name="propertyName">The name of the property to order by.</param>
    /// <param name="isDescending">Indicates whether the sorting should be descending.</param>
    /// <returns>An <see cref="IQueryable{T}"/> whose elements are sorted according to the specified property.</returns>
    public static IQueryable<T> ThenOrderByProperty<T>(this IQueryable<T> source, string propertyName, bool isDescending) => ApplyOrder(source, propertyName, isDescending, true);

    /// <summary>
    /// Converts an <see cref="IQueryable{T}"/> to a <see cref="List{T}"/> asynchronously.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="cancellation">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of elements.</returns>
    public static async ValueTask<List<TSource>> ConvertToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellation)
    {
        if (source is not IAsyncEnumerable<TSource> asyncEnumerable)
        {
            return await Task.Run(() =>
            {
                List<TSource> list1 = [];
                foreach (var source1 in source) list1.Add(source1);
                return list1;
            }, cancellation).ConfigureAwait(false);
        }

        var list = new List<TSource>();
        await foreach (var element in asyncEnumerable.WithCancellation(cancellation).ConfigureAwait(false))
        {
            list.Add(element);
        }

        return list;
    }

    /// <summary>
    /// Converts an <see cref="IQueryable{T}"/> to an <see cref="ImmutableArray{T}"/> asynchronously.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="cancellation">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the immutable array of elements.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the source is not asynchronous.</exception>
    public static async ValueTask<ImmutableArray<TSource>> ConvertToImmutableArrayAsync<TSource>(
        this IQueryable<TSource> source,
        CancellationToken cancellation)
    {
        if (source is not IAsyncEnumerable<TSource> asyncEnumerable)
        {
            throw new InvalidOperationException("IQueryable is not async");
        }

        var builder = ImmutableArray.CreateBuilder<TSource>();
        await foreach (var element in asyncEnumerable.WithCancellation(cancellation).ConfigureAwait(false))
        {
            builder.Add(element);
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Asynchronously counts the number of elements in a sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to count.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of elements in the sequence.</returns>
    public static async Task<int> CountAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        return await Task.Run(source.Count, cancellationToken).ConfigureAwait(false);
    }
}
