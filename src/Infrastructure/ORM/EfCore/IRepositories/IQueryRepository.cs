using Domain.Builders;
using Mapster;
using Shared.Entities;
using Shared.Utils;
using System.Data;
using System.Linq.Expressions;

/// <summary>
/// Represents a repository interface for querying entities of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
public interface IQueryRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByIdAsync(
        TKey id,
        bool enableTracking = false,
        Action<IFluentBuilder<TEntity>>? builder = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a projected entity by its identifier.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="projector">A function to project the entity.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The projected entity if found; otherwise, null.</returns>
    Task<TProjector?> GetByIdAsync<TProjector>(
        TKey id,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a projected entity by its identifier.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="ids">The identifier a list of the entity.</param>
    /// <param name="projector">A function to project the entity.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The projected entity if found; otherwise, null.</returns>
    Task<IEnumerable<TProjector>> GetByIdAsync<TProjector>(
        List<TKey> ids,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="ids">The identifier a list of the entity.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<IEnumerable<TEntity?>> GetByIdAsync(
        List<TKey> ids,
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="paging">Paging information.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of entities.</returns>
    Task<(IEnumerable<TEntity> Entities, Pagination Pagination)> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        Pagination? paging = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all projected entities matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of projected entities.</returns>
    Task<(IEnumerable<TProjector> Entities, Pagination Pagination)> GetAllAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        Pagination? paging = null,
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
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the first projected entity matching the specified predicate.
    /// </summary>
    /// <typeparam name="TProjector">The type of the projection.</typeparam>
    /// <param name="predicate">A predicate to filter the entities.</param>
    /// <param name="projector">A function to project the entities.</param>
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The first projected entity if found; otherwise, null.</returns>
    Task<TProjector?> FirstOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
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
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The single projected entity if found; otherwise, null.</returns>
    Task<TProjector?> SingleOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
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
    /// <param name="builder">A builder to customize the query.</param>
    /// <param name="typeAdapterConfig">The configuration for the TypeAdapter.</param>
    /// <param name="enableTracking">Indicates whether tracking should be disabled.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The last projected entity if found; otherwise, null.</returns>
    Task<TProjector?> LastOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
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

    /// <summary>
    /// Executes a SQL query and returns all results as a list of <typeparamref name="TView"/>.
    /// </summary>
    /// <typeparam name="TView">The type of the view model to map results to.</typeparam>
    /// <param name="sqlQuery">The SQL query string.</param>
    /// <param name="param">Optional query parameters.</param>
    /// <returns>A list of <typeparamref name="TView"/>.</returns>
    Task<List<TView>> GetAllAsync<TView>(string sqlQuery, object? param = null);

    /// <summary>
    /// Executes a command that does not return rows (e.g., INSERT, UPDATE, DELETE).
    /// </summary>
    /// <param name="sql">The SQL command string.</param>
    /// <param name="param">Optional command parameters.</param>
    /// <param name="transaction">Optional database transaction.</param>
    /// <param name="commandTimeout">Optional timeout in seconds.</param>
    /// <param name="commandType">Optional command type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    ValueTask<int> ExecuteAsync(
        string sql,
        object? param = null,
        Pagination? paging = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes a query and returns a data reader for manual processing.
    /// </summary>
    /// <param name="sql">The SQL query string.</param>
    /// <param name="param">Optional query parameters.</param>
    /// <param name="transaction">Optional database transaction.</param>
    /// <param name="commandTimeout">Optional timeout in seconds.</param>
    /// <param name="commandType">Optional command type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="IDataReader"/> for reading results.</returns>
    ValueTask<IDataReader> ExecuteReaderAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes a query and returns the first row or default if none.
    /// </summary>
    ValueTask<TResult?> QueryFirstOrDefault<TResult>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes a query and returns a single row or default if none.
    /// </summary>
    ValueTask<TResult?> QuerySingleOrDefaultAsync<TResult>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes a query and maps results directly to <typeparamref name="TEntity"/>.
    /// </summary>
    ValueTask<IEnumerable<TResult>> QueryAsync<TResult>(
        string rawQuery,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes a scalar query and returns the result.
    /// </summary>
    ValueTask<TResult?> ExecuteScalarAsync<TResult>(
        string rawQuery,
        object? parameters = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Multi-mapping query for two types.
    /// </summary>
    ValueTask<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
        string sql,
        Func<TFirst, TSecond, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Multi-mapping query for three types.
    /// </summary>
    ValueTask<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
        string sql,
        Func<TFirst, TSecond, TThird, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Multi-mapping query for four types.
    /// </summary>
    ValueTask<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
        string sql,
        Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Multi-mapping query for five types.
    /// </summary>
    ValueTask<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
        string sql,
        Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Multi-mapping query for six types.
    /// </summary>
    ValueTask<IEnumerable<TReturn>> QueryAsync<
        TFirst,
        TSecond,
        TThird,
        TFourth,
        TFifth,
        TSixth,
        TReturn
    >(
        string sql,
        Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes a batch query and returns two result sets.
    /// </summary>
    ValueTask<(IEnumerable<T1>, IEnumerable<T2>)> QueryMultiple<T1, T2>(
        string sql,
        object? param = null
    );

    /// <summary>
    /// Executes a batch query and returns three result sets.
    /// </summary>
    ValueTask<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> QueryMultiple<T1, T2, T3>(
        string sql,
        object? param = null
    );

    /// <summary>
    /// Executes a batch query and returns four result sets.
    /// </summary>
    ValueTask<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)> QueryMultiple<
        T1,
        T2,
        T3,
        T4
    >(string sql, object? param = null);
}
