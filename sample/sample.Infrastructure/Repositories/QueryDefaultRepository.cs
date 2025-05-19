using EfCore.Repositories;
using sample.Infrastructure.Persistence.EfCore;
using Shard.Entities;

namespace sample.Infrastructure.Repositories;
public class QueryDefaultRepository<TEntity, TKey>(ApplicationDbContext dbContext)
    : QueryRepository<TEntity, TKey>(dbContext)
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
}