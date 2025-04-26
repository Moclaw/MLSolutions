namespace Domain.IRepositories;

/// <summary>
/// Interface for command repository operations.
/// Provides methods for adding, updating, deleting, and removing entities.
/// </summary>
public interface ICommandRepository
{
    /// <summary>
    /// Adds a single entity asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The added entity.</returns>
    Task<TEntity> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Adds multiple entities asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The added entities.</returns>
    Task<TEntity> AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Updates a single entity asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The updated entity.</returns>
    Task<TEntity> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Updates multiple entities asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The updated entities.</returns>
    Task<TEntity> UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Deletes a single entity asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The deleted entity.</returns>
    Task<TEntity> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Deletes multiple entities asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The deleted entities.</returns>
    Task<TEntity> DeleteRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Removes a single entity asynchronously without deleting it from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to remove.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The removed entity.</returns>
    Task<TEntity> RemoveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Removes multiple entities asynchronously without deleting them from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The entities to remove.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The removed entities.</returns>
    Task<TEntity> RemoveRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;
}
