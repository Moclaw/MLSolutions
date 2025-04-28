using System.Linq.Expressions;
using Domain.IRepositories.Builders;
using EfCore.Builders;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shard.Entities;

namespace EfCore.Repositories;
public class QueryRepository<TEntity, TKey>(BaseDbContext context) : IQueryRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    private DbSet<TEntity> Collection => context.Set<TEntity>();

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
            return await queryBuilder
                .Query.Where(x => x.Id.Equals(id))
                .ProjectToType<TProjector>(typeAdapterConfig)
                .FirstOrDefaultAsync(cancellationToken);
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
            return await queryBuilder
                .Query.Where(x => ids.Contains(x.Id))
                .ProjectToType<TProjector>(typeAdapterConfig)
                .ToListAsync(cancellationToken);
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
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query != null && predicate != null)
            return await queryBuilder.Query.Where(predicate).ToListAsync(cancellationToken);
        return [];
    }

    public async Task<IEnumerable<TProjector>> GetAllAsync<TProjector>(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TProjector>>? projector = null,
        Action<IFluentBuilder<TEntity>>? builder = null,
        TypeAdapterConfig? typeAdapterConfig = null,
        bool enableTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryBuilder = InvokeQueryBuilder(builder, enableTracking);
        if (queryBuilder.Query == null) return [];

        if (predicate != null)
            return await queryBuilder
                .Query.Where(predicate)
                .ProjectToType<TProjector>(typeAdapterConfig)
                .ToListAsync(cancellationToken);

        return [];
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
        if (queryBuilder.Query == null) return default;
        if (predicate != null)
            return await queryBuilder
                .Query.Where(predicate)
                .ProjectToType<TProjector>(typeAdapterConfig)
                .FirstOrDefaultAsync(cancellationToken);

        return default;
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
        if (queryBuilder.Query == null) return default;
        if (predicate != null)
            return await queryBuilder
                .Query.Where(predicate)
                .ProjectToType<TProjector>(typeAdapterConfig)
                .SingleOrDefaultAsync(cancellationToken);

        return default;
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
        if (queryBuilder.Query == null) return default;
        if (predicate != null)
            return await queryBuilder
                .Query.Where(predicate)
                .ProjectToType<TProjector>(typeAdapterConfig)
                .LastOrDefaultAsync(cancellationToken);

        return default;
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
}