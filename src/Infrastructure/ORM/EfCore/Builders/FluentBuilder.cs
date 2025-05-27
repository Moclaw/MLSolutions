using Domain.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MLSolutions;
using Shared.Entities;
using System.Linq.Expressions;

namespace EfCore.Builders;
/// <summary>
/// A fluent builder for constructing dynamic IQueryable queries with ordering, including, and filter ignoring support.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
internal class FluentBuilder<TEntity> : IFluentBuilder<TEntity>
    where TEntity : class, IEntity // Ensure TEntity implements IEntity
{
    private IQueryable<TEntity> _query;

    protected IOrderedQueryable<TEntity>? _order;

    protected readonly FluentBuilder<TEntity>? _root;

    internal IOrderedQueryable<TEntity>? QueryOrder => _order;

    internal virtual IQueryable<TEntity> Query
    {
        get { return _query; }
        set { _query = value; }
    }

    internal FluentBuilder(IQueryable<TEntity> query, IOrderedQueryable<TEntity>? order = default,
        FluentBuilder<TEntity>? root = default)
    {
        _query = query;
        _order = order;
        _root = root;
    }

    internal void UpdateQuery(IQueryable<TEntity> updateQuery) => _query = updateQuery;

    public virtual IFluentBuilder<TEntity> Order<TProperty>(IComparer<TEntity>? comparer = default)
    {
        _query = comparer is not null ? _query.Order(comparer) : _query.Order();
        _root?.UpdateQuery(_query);
        return this;
    }

    public virtual IFluentBuilder<TEntity> OrderBy<TProperty>(Expression<Func<TEntity, TProperty>> keySelector)
    {
        _order = _order is null ? _query.OrderBy(keySelector) : _order.ThenBy(keySelector);
        _query = _order;

        _root?.UpdateQuery(_query);
        return this;
    }

    public virtual IFluentBuilder<TEntity> OrderDescending<TProperty>(IComparer<TEntity>? comparer = default)
    {
        _query = comparer is not null ? _query.OrderDescending(comparer) : _query.OrderDescending();

        _root?.UpdateQuery(_query);
        return this;
    }

    public virtual IFluentBuilder<TEntity> OrderByDescending<TProperty>(
        Expression<Func<TEntity, TProperty>> keySelector)
    {
        _order = _order is null ? _query.OrderByDescending(keySelector) : _order.ThenByDescending(keySelector);
        _query = _order;

        _root?.UpdateQuery(_query);
        return this;
    }

    public virtual IFluentBuilder<TEntity, TProperty, TProperty> Include<TProperty>(
        Expression<Func<TEntity, TProperty?>> navigationProperty)
        where TProperty : class, IEntity
    {
        _query = _query.Include(navigationProperty);
        _root?.UpdateQuery(_query);
        var builder =
            new FluentBuilder<TEntity, TProperty, TProperty>((_root ?? this).Query, (_root ?? this).QueryOrder,
                _root ?? this);
        return builder;
    }

    public virtual IFluentBuilder<TEntity, IEnumerable<TProperty>, TProperty> Include<TProperty>(
        Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty)
    {
        _query = _query.Include(navigationProperty);
        _root?.UpdateQuery(_query);
        var builder = new FluentBuilder<TEntity, IEnumerable<TProperty>, TProperty>((_root ?? this).Query,
            (_root ?? this).QueryOrder, _root ?? this);
        return builder;
    }

    public virtual IFluentBuilder<TEntity> IgnoreFilter()
    {
        _query = _query.IgnoreQueryFilters();
        _root?.UpdateQuery(_query);
        return this;
    }

    public IFluentBuilder<TEntity> Include(string navigationPropertyPath)
    {
        _query = _query.Include(navigationPropertyPath);
        _root?.UpdateQuery(_query);
        return this;
    }

    public IFluentBuilder<TEntity> OrderByProperty(string propertyName, bool isDescending)
    {
        _query = _order is null
            ? _query.OrderByProperty(propertyName, isDescending)
            : _query.ThenOrderByProperty(propertyName, isDescending);
        _order = _query as IOrderedQueryable<TEntity>;

        _root?.UpdateQuery(_query);
        return this;
    }
}

internal class FluentBuilder<TEntity, TProperty, TGeneric>(
    IQueryable<TEntity> query,
    IOrderedQueryable<TEntity>? order,
    FluentBuilder<TEntity> root)
    : FluentBuilder<TEntity>(query, order, root), IFluentBuilder<TEntity, TProperty, TGeneric>
    where TEntity : class, IEntity
{
    public IFluentBuilder<TEntity, IEnumerable<TNextProperty>, TNextProperty> ThenInclude<TNextProperty>(
        Expression<Func<TGeneric, IEnumerable<TNextProperty>>> navigationProperty
    )
    {
        if (_root?.Query is IIncludableQueryable<TEntity, TGeneric> singleInclude)
        {
            _root.Query = singleInclude.ThenInclude(navigationProperty);

            return new FluentBuilder<TEntity, IEnumerable<TNextProperty>, TNextProperty>(
                _root.Query,
                _root.QueryOrder,
                _root
            );
        }

        if (_root?.Query is IIncludableQueryable<TEntity, IEnumerable<TGeneric>> collectionInclude)
        {
            _root.Query = collectionInclude.ThenInclude(navigationProperty);

            return new FluentBuilder<TEntity, IEnumerable<TNextProperty>, TNextProperty>(
                _root.Query,
                _root.QueryOrder,
                _root
            );
        }

        throw new InvalidOperationException("Theninclude do not support type");
    }

    public IFluentBuilder<TEntity, TNextProperty, TNextProperty> ThenInclude<TNextProperty>(
        Expression<Func<TGeneric, TNextProperty?>> navigationProperty
    )
        where TNextProperty : class, IEntity
    {
        if (_root?.Query is IIncludableQueryable<TEntity, TGeneric> singleInclude)
        {
            _root.Query = singleInclude.ThenInclude(navigationProperty);

            return new FluentBuilder<TEntity, TNextProperty, TNextProperty>(
                _root.Query,
                _order,
                _root
            );
        }

        if (_root?.Query is IIncludableQueryable<TEntity, IEnumerable<TGeneric>> collectionInclude)
        {
            _root.Query = collectionInclude.ThenInclude(navigationProperty);

            return new FluentBuilder<TEntity, TNextProperty, TNextProperty>(
                _root.Query,
                _order,
                _root
            );
        }

        throw new InvalidOperationException("Theninclude do not support type");
    }
}