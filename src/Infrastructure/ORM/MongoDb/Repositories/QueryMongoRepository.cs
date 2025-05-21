using Domain.Builders;
using Domain.IRepositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using System.Linq.Expressions;

namespace MongoDb.Repositories;

public class QueryMongoRepository<TEntity, TKey>(MongoBaseContext context)
    : IQueryMongoRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    private DbSet<TEntity> Collection => context.Set<TEntity>();

    public virtual string CollectionName => typeof(TEntity).Name;

    private IQueryable<TEntity> GetQueryable(bool enableTracking = false)
    {
        var query = Collection.AsQueryable();

        if (!enableTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    private static IQueryable<TEntity> BindPredicate(
        IQueryable<TEntity> source,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool shouldIgnoreFilter = false
    )
    {
        if (predicate is null)
        {
            return source;
        }

        if (shouldIgnoreFilter)
        {
            source = source.IgnoreQueryFilters();
        }

        return source.Where(predicate);
    }

    public Task<TEntity?> GetByIdAsync(
        TKey id,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        return GetQueryable(enableTracking)
            .FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }

    public Task<TProjector?> GetByIdAsync<TProjector>(
        TKey id,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable(enableTracking);

        return query
            .Where(e => e.Id.Equals(id))
            .ProjectToType<TProjector>(typeAdapterConfig)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable(enableTracking);

        if (predicate != null)
        {
            query = BindPredicate(query, predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TProjector>> GetByIdAsync<TProjector>(
        List<TKey> ids,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable(enableTracking);

        var list = await query
            .Where(e => ids.Contains(e.Id))
            .ProjectToType<TProjector>(typeAdapterConfig)
            .ToListAsync(cancellationToken);
        return list;
    }

    public async Task<IEnumerable<TProjector>> GetAllAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable(enableTracking);

        if (predicate != null)
        {
            query = BindPredicate(query, predicate);
        }

        return await query
            .ProjectToType<TProjector>(typeAdapterConfig)
            .ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable(enableTracking);

        if (predicate != null)
        {
            query = BindPredicate(query, predicate);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TProjector?> FirstOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable(enableTracking);

        if (predicate != null)
        {
            query = BindPredicate(query, predicate);
        }

        return await query
            .ProjectToType<TProjector>(typeAdapterConfig)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable(enableTracking);

        if (predicate != null)
        {
            query = BindPredicate(query, predicate);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public Task<TProjector?> SingleOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public async Task<TEntity?> LastOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable(enableTracking);

        if (predicate != null)
        {
            query = BindPredicate(query, predicate);
        }

        return await query.LastOrDefaultAsync(cancellationToken);
    }

    public async Task<TProjector?> LastOrDefaultAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable(enableTracking);

        if (predicate != null)
        {
            query = BindPredicate(query, predicate);
        }

        return await query
            .ProjectToType<TProjector>(typeAdapterConfig)
            .LastOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable();

        if (predicate != null)
        {
            query = BindPredicate(query, predicate);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> AllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable();

        if (predicate != null)
        {
            return await query.AllAsync(predicate, cancellationToken);
        }
        return false;
    }

    public Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = GetQueryable();

        if (predicate != null)
        {
            query = BindPredicate(query, predicate);
        }

        return query.CountAsync(cancellationToken);
    }
}
