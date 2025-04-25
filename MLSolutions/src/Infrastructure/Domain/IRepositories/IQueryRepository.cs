using Shard.Entities;

namespace Domain.IRepositories;

public interface IQueryRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<TProjector?> GetByIdAsync<TProjector>(TKey id,
        Func<IQueryable<TEntity>, IQueryable<TProjector>> projector,
        bool disableTracking = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate,
        CancellationToken cancellationToken = default);

}
