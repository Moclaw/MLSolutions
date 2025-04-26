using System.Linq.Expressions;
using Domain.IRepositories.Builders;
using Microsoft.EntityFrameworkCore;
using Shard.Entities;

namespace EfCore.Builders;

internal class FluentBuilder<TEntity>(
    IQueryable<TEntity> query,
    IOrderedQueryable<TEntity>? order = default,
    IFluentBuilder<TEntity>? root = default
) : IFluentBuilder<TEntity>
    where TEntity : class, IEntity
{
    private IQueryable<TEntity> _query = query;

    protected IOrderedQueryable<TEntity>? _order = order;

    protected readonly IFluentBuilder<TEntity>? _root = root;

    internal IOrderedQueryable<TEntity>? QueryOrder => _order;

    internal virtual IQueryable<TEntity> Query
    {
        get { return _query; }
        set { _query = value; }
    }

    internal void UpdateQuery(IQueryable<TEntity> updateQuery)
    {
        _query = updateQuery;
    }

    public IFluentBuilder<TEntity> IgnoreFilter()
    {
        _query = _query.IgnoreQueryFilters();
        (_root as FluentBuilder<TEntity>)?.UpdateQuery(_query);
        return this;
    }

    public IFluentBuilder<TEntity> Include<TProperty>(
        Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty
    )
    {
        var include = _query.Include(navigationProperty);
        var fluentBuilder = new FluentBuilder<TEntity>(
            include,
            _order,
            this
        );
        (_root as FluentBuilder<TEntity>)?.UpdateQuery(include);
        return fluentBuilder;
    }

    public IFluentBuilder<TEntity> Include(string navigationPropertyPath)
    {
        throw new NotImplementedException();
    }

    public IFluentBuilder<TEntity> Order<TProperty>(IComparer<TEntity>? comparer = null)
    {
        throw new NotImplementedException();
    }

    public IFluentBuilder<TEntity> OrderBy<TProperty>(
        Expression<Func<TEntity, TProperty>> keySelector
    )
    {
        throw new NotImplementedException();
    }

    public IFluentBuilder<TEntity> OrderByDescending<TProperty>(
        Expression<Func<TEntity, TProperty>> keySelector
    )
    {
        throw new NotImplementedException();
    }

    public IFluentBuilder<TEntity> OrderByProperty(string propertyName, bool isDescending)
    {
        throw new NotImplementedException();
    }

    public IFluentBuilder<TEntity> OrderDescending<TProperty>(IComparer<TEntity>? comparer = null)
    {
        throw new NotImplementedException();
    }

    IFluentBuilder<TEntity, TProperty, TProperty> IFluentBuilder<TEntity>.Include<TProperty>(
        Expression<Func<TEntity, TProperty?>> navigationProperty
    )
        where TProperty : class
    {
        throw new NotImplementedException();
    }
}
