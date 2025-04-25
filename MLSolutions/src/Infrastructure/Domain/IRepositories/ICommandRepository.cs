using Shard.Entities;

namespace Domain.IRepositories;

public interface ICommandRepository
{
    Task<TEntity> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    Task<TEntity> AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;

    Task<TEntity> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    Task<TEntity> UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;

    Task<TEntity> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    Task<TEntity> DeleteRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;

    Task<TEntity> RemoveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    Task<TEntity> RemoveRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;

}
