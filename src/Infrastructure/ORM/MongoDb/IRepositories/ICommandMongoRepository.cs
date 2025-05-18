using Microsoft.AspNetCore.Http;
using Shard.Entities;
using Shard.Responses;

namespace Domain.IRepositories;

/// <summary>
/// Interface for MongoDB command repository operations.
/// </summary>
public interface ICommandMongoRepository
{
    /// <summary>
    /// Adds a single entity asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with the added entity as the result.</returns>
    ValueTask<TEntity> AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : class, IEntity;

    /// <summary>
    /// Adds multiple entities asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with the added entities as the result.</returns>
    ValueTask<IEnumerable<TEntity>> AddRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class, IEntity;

    /// <summary>
    /// Updates a single entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to update.</param>
    void Update<TEntity>(TEntity entity)
        where TEntity : class, IEntity;

    /// <summary>
    /// Updates multiple entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The entities to update.</param>
    void UpdateRange<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, IEntity;

    /// <summary>
    /// Removes a single entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to remove.</param>
    void Remove<TEntity>(TEntity entity)
        where TEntity : class, IEntity;

    /// <summary>
    /// Removes multiple entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The entities to remove.</param>
    void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, IEntity;

    /// <summary>
    /// Saves changes asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with a response indicating the result.</returns>
    ValueTask<int> SaveChangeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
