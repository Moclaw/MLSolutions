using Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using Shard.Entities;
using Shard.Responses;
using Shard.Utils;

namespace MongoDb.Repositories;

public class CommandMongoReposiory(
    MongoBaseContext context,
    ILogger<CommandMongoReposiory> logger,
    IDbContextTransaction? currentTransaction
) : ICommandMongoRepository
{
    private IDbContextTransaction? _currentTransaction = currentTransaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await EnsureCreatedIndex<IEntity>();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        await _currentTransaction.CommitAsync(cancellationToken);
    }

    public async ValueTask<TEntity> AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TEntity : class, IEntity
    {
        await context.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async ValueTask<IEnumerable<TEntity>> AddRangeAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
        where TEntity : class, IEntity
    {
        var entitiesList = entities.ToList();
        await context.AddRangeAsync(entitiesList, cancellationToken);
        return entitiesList; // Return the list of entities
    }

    public void Update<TEntity>(TEntity entity)
        where TEntity : class, IEntity
    {
        context.Update(entity);
    }

    public void UpdateRange<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, IEntity
    {
        var entitiesList = entities.ToList();
        context.UpdateRange(entitiesList);
    }

    public void Remove<TEntity>(TEntity entity)
        where TEntity : class, IEntity
    {
        context.Remove(entity);
    }

    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, IEntity
    {
        var entitiesList = entities.ToList();
        context.RemoveRange(entitiesList);
    }

    public async ValueTask<int> SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency error occurred while saving changes.");
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.RollbackTransactionAsync(cancellationToken);
            }

            throw new DbUpdateConcurrencyException(
                "Concurrency error occurred while saving changes.",
                ex
            );
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update error occurred while saving changes.");
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.RollbackTransactionAsync(cancellationToken);
            }

            throw new DbUpdateException("Database update error occurred while saving changes.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while saving changes.");
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.RollbackTransactionAsync(cancellationToken);
            }
            throw new Exception("An error occurred while saving changes.", ex);
        }
    }

    #region private

    private async Task EnsureCreatedIndex<TEntity>()
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        if (entityType == null)
            return;

        var collectionName = entityType.GetCollectionName();

        var collection = context.MongoDatabase!.GetCollection<TEntity>(collectionName);

        if (collection == null)
            return;

        var indexes = await collection.Indexes.ListAsync();

        if (!await indexes.MoveNextAsync())
            return;

        var existIndexName = indexes.Current.Select(x => x.Values.Last().AsString).ToList();

        var notExistIndex = entityType
            .GetIndexes()
            .Where(x =>
                x.Properties.Count > 0
                && !string.IsNullOrEmpty(x.Properties[0].Name)
                && !existIndexName.Any(i =>
                    i.Contains(x.Properties[0].Name, StringComparison.OrdinalIgnoreCase)
                )
            )
            .ToList();

        foreach (var index in notExistIndex)
        {
            if (index.Properties.Count == 0)
                continue;

            var first = index.Properties.FirstOrDefault();

            var indexKeysDefinition = Builders<TEntity>.IndexKeys.Ascending(first!.Name);

            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<TEntity>(
                    indexKeysDefinition,
                    new CreateIndexOptions
                    {
                        SphereIndexVersion = 2,
                        Unique = index.IsUnique,
                        TextIndexVersion = 2,
                    }
                )
            );
        }
    }

    #endregion
}
