using Shard.Entities;
using System.Linq.Expressions;

namespace Domain.IRepositories.Builders;
public interface IEntitySetter<TEntity> where TEntity : class, IEntity
{
    IEntitySetter<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> expression, TProperty value);
}