using Shard.Entities;
using System.Linq.Expressions;

namespace Domain.IRepositories.Builders;
/// <summary>
/// Provides methods to build and apply ordering to a queryable collection of entities.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being ordered. Must implement <see cref="IEntity"/>.</typeparam>
public interface IOrderBuilder<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// Applies a custom order to the entities using the specified comparer.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property used for ordering.</typeparam>
    /// <param name="comparer">An optional comparer to define the custom order. If null, the default comparer is used.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity}"/> for further query building.</returns>
    IFluentBuilder<TEntity> Order<TProperty>(IComparer<TEntity>? comparer = null);

    /// <summary>
    /// Applies a custom descending order to the entities using the specified comparer.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property used for ordering.</typeparam>
    /// <param name="comparer">An optional comparer to define the custom descending order. If null, the default comparer is used.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity}"/> for further query building.</returns>
    IFluentBuilder<TEntity> OrderDescending<TProperty>(IComparer<TEntity>? comparer = null);

    /// <summary>
    /// Orders the entities by the specified key selector in ascending order.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property used for ordering.</typeparam>
    /// <param name="keySelector">An expression to select the key for ordering.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity}"/> for further query building.</returns>
    IFluentBuilder<TEntity> OrderBy<TProperty>(Expression<Func<TEntity, TProperty>> keySelector);

    /// <summary>
    /// Orders the entities by the specified key selector in descending order.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property used for ordering.</typeparam>
    /// <param name="keySelector">An expression to select the key for ordering.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity}"/> for further query building.</returns>
    IFluentBuilder<TEntity> OrderByDescending<TProperty>(Expression<Func<TEntity, TProperty>> keySelector);

    /// <summary>
    /// Orders the entities by the specified property name, with an option to order in descending order.
    /// </summary>
    /// <param name="propertyName">The name of the property to order by.</param>
    /// <param name="isDescending">A boolean indicating whether the order should be descending. If false, the order is ascending.</param>
    /// <returns>An instance of <see cref="IFluentBuilder{TEntity}"/> for further query building.</returns>
    IFluentBuilder<TEntity> OrderByProperty(string propertyName, bool isDescending);
}
