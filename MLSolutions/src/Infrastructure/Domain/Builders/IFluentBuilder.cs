using Shard.Entities;
using System.Linq.Expressions;

namespace Domain.IRepositories.Builders;

/// <summary>
/// Provides methods to build queries for entities, including navigation properties and filters.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being queried. Must implement <see cref="IEntity"/>.</typeparam>
public interface IFluentBuilder<TEntity> : IOrderBuilder<TEntity>
   where TEntity : class, IEntity
{
    /// <summary>
    /// Includes a navigation property in the query.
    /// </summary>
    /// <typeparam name="TProperty">The type of the navigation property to include.</typeparam>
    /// <param name="navigationProperty">An expression specifying the navigation property to include.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity, TProperty, TProperty}"/> for further query building.</returns>
    IFluentBuilder<TEntity, TProperty, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty?>> navigationProperty)
        where TProperty : class, IEntity;

    /// <summary>
    /// Includes a collection navigation property in the query.
    /// </summary>
    /// <typeparam name="TProperty">The type of the elements in the collection navigation property.</typeparam>
    /// <param name="navigationProperty">An expression specifying the collection navigation property to include.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity}"/> for further query building.</returns>
    IFluentBuilder<TEntity, IEnumerable<TProperty>, TProperty> Include<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty);

    /// <summary>
    /// Ignores any global filters applied to the query.
    /// </summary>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity}"/> for further query building.</returns>
    IFluentBuilder<TEntity> IgnoreFilter();

    /// <summary>
    /// Includes a navigation property in the query by its string path.
    /// </summary>
    /// <param name="navigationPropertyPath">The string path of the navigation property to include.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity}"/> for further query building.</returns>
    IFluentBuilder<TEntity> Include(string navigationPropertyPath);
}

/// <summary>
/// Provides methods to build queries for entities, including chained navigation properties.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being queried. Must implement <see cref="IEntity"/>.</typeparam>
/// <typeparam name="TProperty">The type of the current navigation property being included.</typeparam>
/// <typeparam name="TGeneric">The type of the generic property for chaining navigation properties.</typeparam>
public interface IFluentBuilder<TEntity, TProperty, TGeneric> : IFluentBuilder<TEntity>
   where TEntity : class, IEntity
{
    /// <summary>
    /// Includes a subsequent navigation property in the query after a previous include.
    /// </summary>
    /// <typeparam name="TNextProperty">The type of the next navigation property to include.</typeparam>
    /// <param name="navigationProperty">An expression specifying the next navigation property to include.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity, TNextProperty, TNextProperty}"/> for further query building.</returns>
    IFluentBuilder<TEntity, TNextProperty, TNextProperty> ThenInclude<TNextProperty>(Expression<Func<TGeneric, TNextProperty?>> navigationProperty)
        where TNextProperty : class, IEntity;

    /// <summary>
    /// Includes a subsequent collection navigation property in the query after a previous include.
    /// </summary>
    /// <typeparam name="TNextProperty">The type of the elements in the next collection navigation property.</typeparam>
    /// <param name="navigationProperty">An expression specifying the next collection navigation property to include.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity}"/> for further query building.</returns>
    IFluentBuilder<TEntity, IEnumerable<TNextProperty>, TNextProperty> ThenInclude<TNextProperty>(Expression<Func<TGeneric, IEnumerable<TNextProperty>>> navigationProperty);
}
