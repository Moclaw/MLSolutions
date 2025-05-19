using Domain.Builders;
using Mapster;
using Shard.Entities;
using System.Linq.Expressions;

namespace Domain.IRepositories;

public interface IQueryMongoRepository<TEntity, in TKey>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByIdAsync(
        TKey id,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a projected entity by its identifier.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="projector">A function to project the entity.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The projected entity if found; otherwise, null.</returns>
    Task<TProjector?> GetByIdAsync<TProjector>(
        TKey id,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all projected entities matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of projected entities.</returns>
    Task<IEnumerable<TProjector>> GetAllAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the first entity matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The first entity if found; otherwise, null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the first projected entity matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The first projected entity if found; otherwise, null.</returns>
    Task<TProjector?> FirstOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the single entity matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The single entity if found; otherwise, null.</returns>
    Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the single projected entity matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The single projected entity if found; otherwise, null.</returns>
    Task<TProjector?> SingleOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the last entity matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The last entity if found; otherwise, null.</returns>
    Task<TEntity?> LastOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the last projected entity matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The last projected entity if found; otherwise, null.</returns>
    Task<TProjector?> LastOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether any entities match the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if any entities match the predicate; otherwise, false.</returns>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether all entities match the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if all entities match the predicate; otherwise, false.</returns>
    Task<bool> AllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts the number of entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entities matching the predicate.</returns>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    string? CollectionName { get; }
}
