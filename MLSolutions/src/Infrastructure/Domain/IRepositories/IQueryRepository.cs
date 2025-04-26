using Domain.IRepositories.Builders;
using Shard.Entities;

/// <summary>
/// Represents a repository interface for querying entities of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
public interface IQueryRepository<TEntity, in TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByIdAsync(TKey id, bool disableTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a projected entity by its identifier.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="projector">A function to project the entity.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The projected entity if found; otherwise, null.</returns>
    Task<TProjector?> GetByIdAsync<TProjector>(TKey id,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null, bool disableTracking = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(Predicate<TEntity>? predicate = null,
        IFluentBuilder<TEntity>? builder = null, bool disableTracking = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all projected entities matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of projected entities.</returns>
    Task<IEnumerable<TProjector>> GetAllAsync<TProjector>(Predicate<TEntity>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null, IFluentBuilder<TEntity>? builder = null,
        bool disableTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the first entity matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The first entity if found; otherwise, null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(Predicate<TEntity>? predicate = null, IFluentBuilder<TEntity>? builder = null,
        bool disableTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the first projected entity matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The first projected entity if found; otherwise, null.</returns>
    Task<TProjector?> FirstOrDefaultAsync<TProjector>(Predicate<TEntity>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null, IFluentBuilder<TEntity>? builder = null,
        bool disableTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the single entity matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The single entity if found; otherwise, null.</returns>
    Task<TEntity?> SingleOrDefaultAsync(Predicate<TEntity>? predicate = null, IFluentBuilder<TEntity>? builder = null,
        bool disableTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the single projected entity matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The single projected entity if found; otherwise, null.</returns>
    Task<TProjector?> SingleOrDefaultAsync<TProjector>(Predicate<TEntity>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null, IFluentBuilder<TEntity>? builder = null,
        bool disableTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the last entity matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The last entity if found; otherwise, null.</returns>
    Task<TEntity?> LastOrDefaultAsync(Predicate<TEntity>? predicate = null, IFluentBuilder<TEntity>? builder = null,
        bool disableTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the last projected entity matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="disableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The last projected entity if found; otherwise, null.</returns>
    Task<TProjector?> LastOrDefaultAsync<TProjector>(Predicate<TEntity>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null, IFluentBuilder<TEntity>? builder = null,
        bool disableTracking = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entities match the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if any entities match the predicate; otherwise, false.</returns>
    Task<bool> AnyAsync(Predicate<TEntity>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether all entities match the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if all entities match the predicate; otherwise, false.</returns>
    Task<bool> AllAsync(Predicate<TEntity>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entities matching the predicate.</returns>
    Task<int> CountAsync(Predicate<TEntity>? predicate = null, CancellationToken cancellationToken = default);
}