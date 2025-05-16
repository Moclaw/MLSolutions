using System.Data;
using Dapper;
using EfCore.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shard.Responses;
using Shard.Utils;

namespace EfCore.Repositories;

public class CommadRepository(BaseDbContext context, ILogger<CommadRepository> logger)
    : ICommandRepository
{
    private IDbContextTransaction? _currentTransaction;

    public async Task<TEntity> AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        await context.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<TEntity> AddRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        var entitiesList = entities.ToList();
        await context.AddRangeAsync(entitiesList, cancellationToken);
        return entitiesList.FirstOrDefault()!; // Return the first entity (or handle null if needed)
    }

    public async Task<TEntity> UpdateAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        context.Update(entity);
        await Task.CompletedTask;
        return entity;
    }

    public async Task<TEntity> UpdateRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        var enumerable = entities.ToList();
        context.UpdateRange(enumerable);
        await Task.CompletedTask;
        return enumerable.FirstOrDefault()!; // Same style: return first entity
    }

    public async Task<TEntity> DeleteAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        context.Remove(entity);
        await Task.CompletedTask;
        return entity;
    }

    public async Task<IEnumerable<TEntity>> DeleteRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        var entitiesList = entities.ToList();
        context.RemoveRange(entitiesList);
        await Task.CompletedTask;
        return entitiesList;
    }

    public async Task<TEntity> RemoveAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        context.Entry(entity).State = EntityState.Detached;
        await Task.CompletedTask;
        return entity;
    }

    public async Task<IEnumerable<TEntity>> RemoveRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        var entitiesList = entities.ToList();
        foreach (var entity in entitiesList)
        {
            context.Entry(entity).State = EntityState.Detached;
        }

        await Task.CompletedTask;
        return entitiesList;
    }

    public async Task BeginTransactionAsync(
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    )
    {
        _currentTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel,
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    )
    {
        _currentTransaction = await context.Database.BeginTransactionAsync(
            isolationLevel,
            cancellationToken
        );
    }

    public async Task CommitAsync(
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    )
    {
        if (_currentTransaction == null)
        {
            logger.LogWarning("No active transaction to commit.");
            throw new InvalidOperationException("No active transaction to commit.");
        }

        await context.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            logger.LogWarning("No active transaction to commit.");
            throw new InvalidOperationException("No active transaction to commit.");
        }

        await context.SaveChangesAsync(cancellationToken);
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task<Responses> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await context.SaveChangesAsync(cancellationToken);
            return ResponseUtils.Success(message: "Changes saved successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency error occurred while saving changes.");
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.RollbackTransactionAsync(cancellationToken);
            }
            return new Responses(
                IsSuccess: false,
                StatusCode: 409,
                Message: "Concurrency error occurred."
            );
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update error occurred while saving changes.");
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.RollbackTransactionAsync(cancellationToken);
            }
            return new Responses(
                IsSuccess: false,
                StatusCode: 500,
                Message: "Database update error occurred."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while saving changes.");
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.RollbackTransactionAsync(cancellationToken);
            }
            return new Responses(
                IsSuccess: false,
                StatusCode: 500,
                Message: "An error occurred while saving changes."
            );
        }
    }

    public async Task<Responses> SaveChangesAsync(
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var data = await context.SaveChangesAsync(cancellationToken);
            return ResponseUtils.Success(message: "Changes saved successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency error occurred while saving changes.");
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.RollbackTransactionAsync(cancellationToken);
            }
            return new Responses(
                IsSuccess: false,
                StatusCode: 409,
                Message: "Concurrency error occurred."
            );
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update error occurred while saving changes.");
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.RollbackTransactionAsync(cancellationToken);
            }
            return new Responses(
                IsSuccess: false,
                StatusCode: 500,
                Message: "Database update error occurred."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while saving changes.");
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.RollbackTransactionAsync(cancellationToken);
            }
            return new Responses(
                IsSuccess: false,
                StatusCode: 500,
                Message: "An error occurred while saving changes."
            );
        }
    }

    public async Task<int> SaveChangesAsync(
        int expectedCount,
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default
    )
    {
        var affectedRows = await context.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken
        );

        if (affectedRows != expectedCount)
        {
            logger.LogWarning(
                "Expected {ExpectedCount} but got {AffectedRows} rows affected.",
                expectedCount,
                affectedRows
            );
        }

        return affectedRows;
    }

    public async Task<int> ExecuteAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var conn = context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        return await conn.ExecuteAsync(
            sql: sql,
            param: param,
            transaction: transaction,
            commandTimeout: commandTimeout,
            commandType: commandType
        );
    }

    public async Task<TResult?> ExecuteScalarAsync<TResult>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        var conn = context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        return await conn.ExecuteScalarAsync<TResult>(
            sql: sql,
            param: param,
            transaction: transaction,
            commandTimeout: commandTimeout,
            commandType: commandType
        );
    }
}
