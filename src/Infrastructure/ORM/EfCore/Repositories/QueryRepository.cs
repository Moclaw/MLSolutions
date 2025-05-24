using System.Data;
using System.Linq.Expressions;
using Domain.Builders;
using EfCore.Builders;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Shared.Utils;

namespace EfCore.Repositories;

public class QueryRepository<TEntity, TKey>(BaseDbContext context) : IQueryRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    private DbSet<TEntity> Collection => context.Set<TEntity>();

    #region EfCore

    private FluentBuilder<TEntity> InvokeQueryBuilder(
        Action<IFluentBuilder<TEntity>>? querybuilder,
        bool enableNoTracking = true
    )
    {
        var query = enableNoTracking ? Collection.AsNoTracking() : Collection;
        var builder = new FluentBuilder<TEntity>(query);
        querybuilder?.Invoke(builder);
        return builder;
    }

    public async Task<TEntity?> GetByIdAsync(
        TKey id,
        bool enableTracking = false,
        Action<IFluentBuilder<TEntity>>? builder = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query != null)
            return await queryBuilder.Query.FirstOrDefaultAsync(
                x => x.Id.Equals(id),
                cancellationToken
            );
        return null;
    }

    public async Task<TProjector?> GetByIdAsync<TProjector>(
        TKey id,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query != null)
        {
            var query = queryBuilder.Query.Where(x => x.Id.Equals(id));
            var projectedQuery =
                projector != null
                    ? projector(query)
                    : query.ProjectToType<TProjector>(typeAdapterConfig);
            return await projectedQuery.FirstOrDefaultAsync(cancellationToken);
        }

        return default;
    }

    public async Task<IEnumerable<TProjector>> GetByIdAsync<TProjector>(
        List<TKey> ids,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query != null)
        {
            var query = queryBuilder.Query.Where(x => ids.Contains(x.Id));
            var projectedQuery =
                projector != null
                    ? projector(query)
                    : query.ProjectToType<TProjector>(typeAdapterConfig);
            return await projectedQuery.ToListAsync(cancellationToken);
        }

        return [];
    }

    public async Task<IEnumerable<TEntity?>> GetByIdAsync(
        List<TKey> ids,
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query != null)
            return await queryBuilder
                .Query.Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);
        return [];
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        Paging? paging = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query != null)
        {
            var query =
                predicate != null ? queryBuilder.Query.Where(predicate) : queryBuilder.Query;

            // Apply paging if provided
            if (paging != null)
            {
                query = query.Skip((paging.PageIndex - 1) * paging.PageSize).Take(paging.PageSize);
            }

            return await query.ToListAsync(cancellationToken);
        }
        return [];
    }

    public async Task<IEnumerable<TProjector>> GetAllAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        Paging? paging = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query == null)
            return [];

        var query = queryBuilder.Query;
        if (predicate != null)
            query = query.Where(predicate);

        var projectedQuery =
            projector != null
                ? projector(query)
                : query.ProjectToType<TProjector>(typeAdapterConfig);

        // Apply paging if provided
        if (paging != null)
        {
            projectedQuery = projectedQuery
                .Skip((paging.PageIndex - 1) * paging.PageSize)
                .Take(paging.PageSize);
        }

        return await projectedQuery.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query != null && predicate != null)
            return await queryBuilder.Query.FirstOrDefaultAsync(predicate, cancellationToken);
        return null;
    }

    public async Task<TProjector?> FirstOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query == null)
            return default;
        var query = queryBuilder.Query;
        if (predicate != null)
            query = query.Where(predicate);
        var projectedQuery =
            projector != null
                ? projector(query)
                : query.ProjectToType<TProjector>(typeAdapterConfig);
        return await projectedQuery.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query != null && predicate != null)
            return await queryBuilder.Query.SingleOrDefaultAsync(predicate, cancellationToken);
        return null;
    }

    public async Task<TProjector?> SingleOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query == null)
            return default;
        var query = queryBuilder.Query;
        if (predicate != null)
            query = query.Where(predicate);
        var projectedQuery =
            projector != null
                ? projector(query)
                : query.ProjectToType<TProjector>(typeAdapterConfig);
        return await projectedQuery.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> LastOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query != null && predicate != null)
            return await queryBuilder.Query.LastOrDefaultAsync(predicate, cancellationToken);
        return null;
    }

    public async Task<TProjector?> LastOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query == null)
            return default;
        var query = queryBuilder.Query;
        if (predicate != null)
            query = query.Where(predicate);
        var projectedQuery =
            projector != null
                ? projector(query)
                : query.ProjectToType<TProjector>(typeAdapterConfig);
        return await projectedQuery.LastOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(null, false);
        if (queryBuilder.Query != null && predicate != null)
            return await queryBuilder.Query.AnyAsync(predicate, cancellationToken);
        return false;
    }

    public async Task<bool> AllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(null, false);
        if (queryBuilder.Query != null && predicate != null)
            return await queryBuilder.Query.AllAsync(predicate, cancellationToken);
        return false;
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(null, false);
        if (queryBuilder.Query != null && predicate != null)
            return await queryBuilder.Query.CountAsync(predicate, cancellationToken);
        return 0;
    }

    #endregion

    #region Dapper
    public async Task<List<TView>> GetAllAsync<TView>(string sqlQuery, object? param = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(sqlQuery);
        var result = await context.ExecuteQueryAsync<TView>(sqlQuery, param);

        return [.. result];
    }

    public virtual ValueTask<int> ExecuteAsync(
        string sql,
        object? param = null,
        Paging? paging = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }

        string finalSql = sql;
        object? finalParam = param;

        if (paging != null)
        {
            // This is a naive paging implementation for SQL Server. Adjust as needed for your DB.
            finalSql =
                $@"
                SELECT * FROM (
                    {sql}
                ) AS PagedResult
                ORDER BY (SELECT NULL)
                OFFSET {(paging.PageIndex - 1) * paging.PageSize} ROWS
                FETCH NEXT {paging.PageSize} ROWS ONLY
            ";
        }

        return context.ExecuteAsync(
            finalSql,
            finalParam,
            transaction,
            commandTimeout,
            commandType,
            cancellationToken
        );
    }

    public virtual ValueTask<IDataReader> ExecuteReaderAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }

        return context.ExecuteReaderAsync(
            sql,
            param,
            transaction,
            commandTimeout,
            commandType,
            cancellationToken
        );
    }

    public virtual ValueTask<TResult?> QueryFirstOrDefault<TResult>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }

        return context.FirstOrDefault<TResult>(
            sql,
            param,
            transaction,
            commandTimeout,
            commandType,
            cancellationToken
        );
    }

    public virtual ValueTask<TResult?> QuerySingleOrDefaultAsync<TResult>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        return context.SingleOrDefaultAsync<TResult>(
            sql,
            param,
            transaction,
            commandTimeout,
            commandType,
            cancellationToken
        );
    }

    public virtual ValueTask<IEnumerable<TEntities>> QueryAsync<TEntities>(
        string rawQuery,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(rawQuery))
        {
            throw new ArgumentException(
                $"'{nameof(rawQuery)}' cannot be null or empty.",
                nameof(rawQuery)
            );
        }

        return context.ExecuteQueryAsync<TEntities>(rawQuery, parameters, cancellationToken);
    }

    public virtual ValueTask<TResult?> ExecuteScalarAsync<TResult>(
        string rawQuery,
        object? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(rawQuery))
        {
            throw new ArgumentException(
                $"'{nameof(rawQuery)}' cannot be null or empty.",
                nameof(rawQuery)
            );
        }

        return context.ExecuteScalarAsync<TResult?>(rawQuery, parameters, cancellationToken);
    }

    public virtual ValueTask<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
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
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }

        ArgumentNullException.ThrowIfNull(map);

        if (string.IsNullOrEmpty(splitOn))
        {
            throw new ArgumentException(
                $"'{nameof(splitOn)}' cannot be null or empty.",
                nameof(splitOn)
            );
        }

        return context.ExecuteQueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType,
            cancellationToken
        );
    }

    public virtual ValueTask<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
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
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }

        ArgumentNullException.ThrowIfNull(map);

        if (string.IsNullOrEmpty(splitOn))
        {
            throw new ArgumentException(
                $"'{nameof(splitOn)}' cannot be null or empty.",
                nameof(splitOn)
            );
        }

        return context.ExecuteQueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType,
            cancellationToken
        );
    }

    public virtual ValueTask<IEnumerable<TReturn>> QueryAsync<
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
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }

        ArgumentNullException.ThrowIfNull(map);

        if (string.IsNullOrEmpty(splitOn))
        {
            throw new ArgumentException(
                $"'{nameof(splitOn)}' cannot be null or empty.",
                nameof(splitOn)
            );
        }

        return context.ExecuteQueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType,
            cancellationToken
        );
    }

    public virtual ValueTask<IEnumerable<TReturn>> QueryAsync<
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
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }

        ArgumentNullException.ThrowIfNull(map);

        if (string.IsNullOrEmpty(splitOn))
        {
            throw new ArgumentException(
                $"'{nameof(splitOn)}' cannot be null or empty.",
                nameof(splitOn)
            );
        }

        return context.ExecuteQueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType,
            cancellationToken
        );
    }

    public virtual ValueTask<IEnumerable<TReturn>> QueryAsync<
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
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }

        ArgumentNullException.ThrowIfNull(map);

        if (string.IsNullOrEmpty(splitOn))
        {
            throw new ArgumentException(
                $"'{nameof(splitOn)}' cannot be null or empty.",
                nameof(splitOn)
            );
        }

        return context.ExecuteQueryAsync(
            sql,
            map,
            param,
            transaction,
            buffered,
            splitOn,
            commandTimeout,
            commandType,
            cancellationToken
        );
    }

    public ValueTask<(IEnumerable<T1>, IEnumerable<T2>)> QueryMultiple<T1, T2>(
        string sql,
        object? param = null
    )
    {
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }

        return context.ExecuteQueryMultipleAsync<T1, T2>(sql, param);
    }

    public ValueTask<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> QueryMultiple<T1, T2, T3>(
        string sql,
        object? param = null
    )
    {
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }
        return context.ExecuteQueryMultipleAsync<T1, T2, T3>(sql, param);
    }

    public ValueTask<(
        IEnumerable<T1>,
        IEnumerable<T2>,
        IEnumerable<T3>,
        IEnumerable<T4>
    )> QueryMultiple<T1, T2, T3, T4>(string sql, object? param = null)
    {
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException($"'{nameof(sql)}' cannot be null or empty.", nameof(sql));
        }
        return context.ExecuteQueryMultipleAsync<T1, T2, T3, T4>(sql, param);
    }
    #endregion
}
