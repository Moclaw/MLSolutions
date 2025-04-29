using System.Linq.Expressions;
using Domain.IRepositories.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MLSolutions;
using Shard.Entities;

namespace EfCore.Builders;

/// <summary>
/// A fluent builder for constructing dynamic IQueryable queries with ordering, including, and filter ignoring support.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
internal class FluentBuilder<TEntity>(
    IQueryable<TEntity>? query,
    IOrderedQueryable<TEntity>? order = null,
    FluentBuilder<TEntity>? root = null
) : IFluentBuilder<TEntity>
    where TEntity : class, IEntity
{
    private IQueryable<TEntity>? _query = query;

    /// <summary>
    /// Gets or sets the current ordered query.
    /// </summary>
    protected IOrderedQueryable<TEntity>? OrderQuery { get; set; } = order;

    /// <summary>
    /// Gets the root builder instance.
    /// </summary>
    protected FluentBuilder<TEntity> Root { get; } =
        root ?? new FluentBuilder<TEntity>(query, order, root);

    internal IOrderedQueryable<TEntity>? QueryOrder => OrderQuery;

    internal virtual IQueryable<TEntity>? Query
    {
        get => _query;
        set => _query = value;
    }

    private void UpdateQuery(IQueryable<TEntity>? updateQuery)
    {
        _query = updateQuery;
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity> Order<TProperty>(IComparer<TEntity>? comparer = null)
    {
        _query = comparer is not null ? _query?.Order(comparer) : _query?.Order();
        Root.UpdateQuery(_query);
        return this;
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity> OrderDescending<TProperty>(IComparer<TEntity>? comparer = null)
    {
        _query = comparer is not null
            ? _query?.OrderDescending(comparer)
            : _query?.OrderDescending();
        Root.UpdateQuery(_query);
        return this;
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity> OrderBy<TProperty>(
        Expression<Func<TEntity, TProperty>> keySelector
    )
    {
        _query = _query?.OrderBy(keySelector);
        Root.UpdateQuery(_query);
        return this;
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity> OrderByDescending<TProperty>(
        Expression<Func<TEntity, TProperty>> keySelector
    )
    {
        _query = _query?.OrderByDescending(keySelector);
        Root.UpdateQuery(_query);
        return this;
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity> OrderByProperty(string propertyName, bool isDescending)
    {
        _query = OrderQuery is null
            ? _query?.OrderByProperty(propertyName, isDescending)
            : _query?.ThenOrderByProperty(propertyName, isDescending);
        OrderQuery ??= _query as IOrderedQueryable<TEntity>;
        Root.UpdateQuery(_query);
        return this;
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity, TProperty, TProperty> Include<TProperty>(
        Expression<Func<TEntity, TProperty?>> navigationProperty
    )
        where TProperty : class, IEntity
    {
        _query = _query?.Include(navigationProperty);
        Root.UpdateQuery(_query);
        var builder = new FluentBuilder<TEntity, TProperty, TProperty>(
            Root.Query,
            Root.QueryOrder,
            Root
        );
        return builder;
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity, IEnumerable<TProperty>, TProperty> Include<TProperty>(
        Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty
    )
    {
        _query = _query?.Include(navigationProperty);
        Root.UpdateQuery(_query);
        var builder = new FluentBuilder<TEntity, IEnumerable<TProperty>, TProperty>(
            Root.Query,
            Root.QueryOrder,
            Root
        );
        return builder;
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity> IgnoreFilter()
    {
        _query = _query?.IgnoreQueryFilters();
        Root.UpdateQuery(_query);
        return this;
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity> Include(string navigationPropertyPath)
    {
        _query = _query?.Include(navigationPropertyPath);
        Root.UpdateQuery(_query);
        return this;
    }
}

/// <summary>
/// Extended fluent builder that supports ThenInclude for navigation properties.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TProperty">The navigation property type.</typeparam>
/// <typeparam name="TGeneric">The current property type in the navigation chain.</typeparam>
internal class FluentBuilder<TEntity, TProperty, TGeneric>(
    IQueryable<TEntity>? query,
    IOrderedQueryable<TEntity>? order,
    FluentBuilder<TEntity> root
) : FluentBuilder<TEntity>(query, order, root), IFluentBuilder<TEntity, TProperty, TGeneric>
    where TEntity : class, IEntity
{
    /// <inheritdoc/>
    public IFluentBuilder<
        TEntity,
        IEnumerable<TNextProperty>,
        TNextProperty
    > ThenInclude<TNextProperty>(
        Expression<Func<TGeneric, IEnumerable<TNextProperty>>> navigationProperty
    )
    {
        switch (Root.Query)
        {
            case IIncludableQueryable<TEntity, TGeneric> singleInclude:
                Root.Query = singleInclude.ThenInclude(navigationProperty);
                return new FluentBuilder<TEntity, IEnumerable<TNextProperty>, TNextProperty>(
                    Root.Query,
                    Root.QueryOrder,
                    Root
                );
            case IIncludableQueryable<TEntity, IEnumerable<TGeneric>> collectionInclude:
                Root.Query = collectionInclude.ThenInclude(navigationProperty);
                return new FluentBuilder<TEntity, IEnumerable<TNextProperty>, TNextProperty>(
                    Root.Query,
                    Root.QueryOrder,
                    Root
                );
            default:
                throw new InvalidOperationException("Then include do not support type");
        }
    }

    /// <inheritdoc/>
    public IFluentBuilder<TEntity, TNextProperty, TNextProperty> ThenInclude<TNextProperty>(
        Expression<Func<TGeneric, TNextProperty?>> navigationProperty
    )
        where TNextProperty : class, IEntity
    {
        switch (Root.Query)
        {
            case IIncludableQueryable<TEntity, TGeneric> singleInclude:
                Root.Query = singleInclude.ThenInclude(navigationProperty);
                return new FluentBuilder<TEntity, TNextProperty, TNextProperty>(
                    Root.Query,
                    OrderQuery,
                    Root
                );
            case IIncludableQueryable<TEntity, IEnumerable<TGeneric>> collectionInclude:
                Root.Query = collectionInclude.ThenInclude(navigationProperty);
                return new FluentBuilder<TEntity, TNextProperty, TNextProperty>(
                    Root.Query,
                    OrderQuery,
                    Root
                );
            default:
                throw new InvalidOperationException("Then include do not support type");
        }
    }
}
