using Dapper;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using System.Data;
using System.Reflection;

namespace EfCore;

/// <summary>
/// Base DbContext with extended Dapper integration and dynamic configuration loading.
/// </summary>
public abstract class BaseDbContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// Gets the executing assembly to apply configurations.
    /// </summary>
    protected virtual Assembly ExecutingAssembly => Assembly.GetExecutingAssembly();

    /// <summary>
    /// Gets the predicate to filter which configurations should be applied.
    /// </summary>
    protected abstract Func<Type, bool> RegisterConfigurationsPredicate { get; }

    /// <summary>
    /// Automatically maps DbSet properties for all entity types in the assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from the executing assembly
        modelBuilder.ApplyConfigurationsFromAssembly(
            assembly: ExecutingAssembly,
            predicate: RegisterConfigurationsPredicate
        );

        // Dynamically add DbSet properties for all entity types
        var entityTypes = ExecutingAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEntity<>).IsAssignableFrom(t));

        foreach (var entityType in entityTypes)
        {
            modelBuilder.Entity(entityType);
        }
    }

    /// <summary>
    /// Tries to get and open the database connection if not already opened.
    /// </summary>
    /// <returns>An open <see cref="IDbConnection"/>.</returns>
    protected virtual IDbConnection TryGetConnection()
    {
        var connection = Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }
        return connection;
    }

    /// <summary>
    /// Executes a non-query SQL command asynchronously.
    /// </summary>
    public virtual async ValueTask<int> ExecuteAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.ExecuteAsync(
            sql,
            param,
            transaction,
            commandTimeout,
            commandType
        );
        return result;
    }

    /// <summary>
    /// Executes a query and returns a data reader asynchronously.
    /// </summary>
    public virtual async ValueTask<IDataReader> ExecuteReaderAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.ExecuteReaderAsync(
            sql,
            param,
            transaction,
            commandTimeout,
            commandType
        );
        return result;
    }

    /// <summary>
    /// Executes a query and returns the first record or default asynchronously.
    /// </summary>
    public virtual async ValueTask<TResult?> FirstOrDefault<TResult>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<TResult>(
            sql,
            param,
            transaction,
            commandTimeout,
            commandType
        );
        return result;
    }

    /// <summary>
    /// Executes a query and returns a single record or default asynchronously.
    /// </summary>
    public virtual async ValueTask<TResult?> SingleOrDefaultAsync<TResult>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.QuerySingleOrDefaultAsync<TResult>(
            sql,
            param,
            transaction,
            commandTimeout,
            commandType
        );
        return result;
    }

    /// <summary>
    /// Executes a query and maps results to an entity collection asynchronously.
    /// </summary>
    public virtual async ValueTask<IEnumerable<TEntity>> ExecuteQueryAsync<TEntity>(
        string rawQuery,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.QueryAsync<TEntity>(rawQuery, parameters);
        return result;
    }

    /// <summary>
    /// Executes a scalar SQL command asynchronously and returns a result.
    /// </summary>
    public virtual async ValueTask<TResult?> ExecuteScalarAsync<TResult>(
        string rawQuery,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.ExecuteScalarAsync<TResult?>(rawQuery, parameters);
        return result;
    }

    /// <summary>
    /// Executes a multi-mapping query (2 entities) asynchronously and maps results.
    /// </summary>
    public virtual async ValueTask<IEnumerable<TReturn>> ExecuteQueryAsync<
        TFirst,
        TSecond,
        TReturn
    >(
        string sql,
        Func<TFirst, TSecond, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.QueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType
        );
        return result;
    }

    /// <summary>
    /// Executes a multi-mapping query (3 entities) asynchronously and maps results.
    /// </summary>
    public virtual async ValueTask<IEnumerable<TReturn>> ExecuteQueryAsync<
        TFirst,
        TSecond,
        TThird,
        TReturn
    >(
        string sql,
        Func<TFirst, TSecond, TThird, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.QueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType
        );
        return result;
    }

    /// <summary>
    /// Executes a multi-mapping query (4 entities) asynchronously and maps results.
    /// </summary>
    public virtual async ValueTask<IEnumerable<TReturn>> ExecuteQueryAsync<
        TFirst,
        TSecond,
        TThird,
        TFourth,
        TReturn
    >(
        string sql,
        Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.QueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType
        );
        return result;
    }

    /// <summary>
    /// Executes a multi-mapping query (5 entities) asynchronously and maps results.
    /// </summary>
    public virtual async ValueTask<IEnumerable<TReturn>> ExecuteQueryAsync<
        TFirst,
        TSecond,
        TThird,
        TFourth,
        TFifth,
        TReturn
    >(
        string sql,
        Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var connection = TryGetConnection();
        var result = await connection.QueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType
        );
        return result;
    }

    /// <summary>
    /// Executes a multi-mapping query (6 entities) asynchronously and maps results.
    /// </summary>
    public virtual async ValueTask<IEnumerable<TReturn>> ExecuteQueryAsync<
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
    )
    {
        var connection = TryGetConnection();
        var result = await connection.QueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType
        );
        return result;
    }

    /// <summary>
    /// Executes a multiple result set query and maps two result sets.
    /// </summary>
    public virtual async ValueTask<(IEnumerable<T1>, IEnumerable<T2>)> ExecuteQueryMultipleAsync<
        T1,
        T2
    >(string sql, object? param = null)
    {
        var connection = TryGetConnection();
        using var reader = await connection.QueryMultipleAsync(sql, param);
        var result1 = await reader.ReadAsync<T1>();
        var result2 = await reader.ReadAsync<T2>();
        return (result1, result2);
    }

    /// <summary>
    /// Executes a multiple result set query and maps three result sets.
    /// </summary>
    public virtual async ValueTask<(
        IEnumerable<T1>,
        IEnumerable<T2>,
        IEnumerable<T3>
    )> ExecuteQueryMultipleAsync<T1, T2, T3>(string sql, object? param = null)
    {
        var connection = TryGetConnection();
        using var reader = await connection.QueryMultipleAsync(sql, param);
        var result1 = await reader.ReadAsync<T1>();
        var result2 = await reader.ReadAsync<T2>();
        var result3 = await reader.ReadAsync<T3>();
        return (result1, result2, result3);
    }

    /// <summary>
    /// Executes a multiple result set query and maps four result sets.
    /// </summary>
    public virtual async ValueTask<(
        IEnumerable<T1>,
        IEnumerable<T2>,
        IEnumerable<T3>,
        IEnumerable<T4>
    )> ExecuteQueryMultipleAsync<T1, T2, T3, T4>(string sql, object? param = null)
    {
        var connection = TryGetConnection();
        using var reader = await connection.QueryMultipleAsync(sql, param);
        var result1 = await reader.ReadAsync<T1>();
        var result2 = await reader.ReadAsync<T2>();
        var result3 = await reader.ReadAsync<T3>();
        var result4 = await reader.ReadAsync<T4>();
        return (result1, result2, result3, result4);
    }
}
