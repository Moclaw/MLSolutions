using System.Linq.Expressions;
using Shard.Entities;

namespace Domain.IRepositories.Builders;
public interface IEntitySetter<TEntity> where TEntity : class, IEntity
{
    IEntitySetter<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> expression, TProperty value);
}