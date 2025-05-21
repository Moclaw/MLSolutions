using Shared.Entities;
using System.Linq.Expressions;

namespace Domain.Builders;

public interface IFluentBuilder<TEntity> : IOrderBuilder<TEntity>
    where TEntity : class, IEntity
{
    IFluentBuilder<TEntity, TProperty, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty?>> navigationProperty)
        where TProperty : class, IEntity;

    IFluentBuilder<TEntity, IEnumerable<TProperty>, TProperty> Include<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty);

    IFluentBuilder<TEntity> IgnoreFilter();

    IFluentBuilder<TEntity> Include(string navigationPropertyPath);

}

public interface IFluentBuilder<TEntity, TProperty, TGeneric> : IFluentBuilder<TEntity>
    where TEntity : class, IEntity
{
    IFluentBuilder<TEntity, TNextProperty, TNextProperty> ThenInclude<TNextProperty>(Expression<Func<TGeneric, TNextProperty?>> navigationProperty) where TNextProperty : class, IEntity;
    IFluentBuilder<TEntity, IEnumerable<TNextProperty>, TNextProperty> ThenInclude<TNextProperty>(Expression<Func<TGeneric, IEnumerable<TNextProperty>>> navigationProperty);
}
