using System.Data;

namespace EfCore.IRepositories;

/// <summary>
/// The <see cref="ICommandRepository"/> interface defines a standardized contract
/// for performing create, update, delete, and remove operations on entities within a data source.
/// </summary>
/// <remarks>
/// This interface is typically used in the Command side of the CQRS (Command Query Responsibility Segregation) pattern,
/// separating write operations from read operations.
///
/// It includes methods for:
/// <list type="bullet">
/// <item><description>Managing transactions (begin, commit)</description></item>
/// <item><description>Saving changes with expected validation</description></item>
/// <item><description>Adding, updating, deleting, and removing single or multiple entities asynchronously</description></item>
/// </list>
///
/// All operations are asynchronous to promote scalability and responsiveness in high-concurrency environments.
/// Implementations are expected to manage entity states properly within the underlying EfCore (e.g., Entity Framework Core).
/// </remarks>
public interface ICommandRepository
{
    /// <summary>
    /// Begins a database transaction asynchronously.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether all changes should be accepted automatically upon success.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task BeginTransactionAsync(
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Begins a database transaction asynchronously with a specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The isolation level to apply to the transaction.</param>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether all changes should be accepted automatically upon success.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task BeginTransactionAsync(
        IsolationLevel isolationLevel,
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Commits the current transaction asynchronously and optionally accepts all changes.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether all changes should be accepted automatically upon success.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task CommitAsync(
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes asynchronously to the database.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes asynchronously to the database, with optional control over accepting changes on success.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether all changes should be accepted automatically upon success.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Saves all pending changes asynchronously and ensures that a specific number of records were affected.
    /// </summary>
    /// <param name="expectedCount">The expected number of affected records.</param>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether all changes should be accepted automatically upon success.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The actual number of affected records.</returns>
    Task<int> SaveChangesAsync(
        int expectedCount,
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a single entity asynchronously to the context.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The added entity.</returns>
    Task<TEntity> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Adds multiple entities asynchronously to the context.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The collection of entities to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The added entities.</returns>
    Task<TEntity> AddRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class;

    /// <summary>
    /// Updates a single entity asynchronously in the context.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The updated entity.</returns>
    Task<TEntity> UpdateAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : class;

    /// <summary>
    /// Updates multiple entities asynchronously in the context.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The collection of entities to update.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The updated entities.</returns>
    Task<TEntity> UpdateRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class;

    /// <summary>
    /// Permanently deletes a single entity asynchronously from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The deleted entity.</returns>
    Task<TEntity> DeleteAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : class;

    /// <summary>
    /// Permanently deletes multiple entities asynchronously from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The collection of entities to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The deleted entities.</returns>
    Task<IEnumerable<TEntity>> DeleteRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class;

    /// <summary>
    /// Removes a single entity from the context asynchronously without deleting it from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to remove from tracking.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The removed entity.</returns>
    Task<TEntity> RemoveAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : class;

    /// <summary>
    /// Removes multiple entities from the context asynchronously without deleting them from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    /// <param name="entities">The collection of entities to remove from tracking.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The removed entities.</returns>
    Task<IEnumerable<TEntity>> RemoveRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class;

    /// <summary>
    /// Executes a raw SQL command asynchronously and returns the number of rows affected.
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="param">Optional parameters for the SQL command.</param>
    /// <param name="transaction">Optional database transaction to associate with the command.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="commandType">Optional type of the command (e.g., Text, StoredProcedure).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The number of rows affected by the command.</returns>
    Task<int> ExecuteAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes a raw SQL query asynchronously and returns a single scalar value.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">Optional parameters for the SQL query.</param>
    /// <param name="transaction">Optional database transaction to associate with the query.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="commandType">Optional type of the command (e.g., Text, StoredProcedure).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The scalar value resulting from the query.</returns>
    Task<TResult?> ExecuteScalarAsync<TResult>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );
}
