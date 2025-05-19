using Shard.Entities;
using System.Linq.Expressions;

namespace Domain.Builders;

public interface IOrderBuilder<TEntity> where TEntity : class, IEntity
{
    IFluentBuilder<TEntity> Order<TProperty>(IComparer<TEntity>? comparer = default);
    IFluentBuilder<TEntity> OrderDescending<TProperty>(IComparer<TEntity>? comparer = default);

    IFluentBuilder<TEntity> OrderBy<TProperty>(Expression<Func<TEntity, TProperty>> keySelector);
    IFluentBuilder<TEntity> OrderByDescending<TProperty>(Expression<Func<TEntity, TProperty>> keySelector);

    IFluentBuilder<TEntity> OrderByProperty(string propertyName, bool isDescending);
}
