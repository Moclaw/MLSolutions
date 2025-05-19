using Domain.IRepositories.Builders;
using Shard.Entities;
using System.Linq.Expressions;

namespace EfCore.Builders;

internal sealed class EntitySetter<TEntity>(TEntity entity) : IEntitySetter<TEntity>
    where TEntity : class, IEntity
{
    private static HashSet<string> UpdateProperties => [];

    private TEntity Item { get; } = entity;

    public IEntitySetter<TEntity> Set<TProperty>(
        Expression<Func<TEntity, TProperty>> expression,
        TProperty value
    )
    {
        var propertyExpression = (MemberExpression)(
            expression.Body is UnaryExpression unary ? unary.Operand : expression.Body
        );
        var propertyName = propertyExpression.Member.Name;

        UpdateProperties.Add(propertyName);

        typeof(TEntity).GetProperty(propertyName)?.SetValue(Item, value);
        return this;
    }
}
