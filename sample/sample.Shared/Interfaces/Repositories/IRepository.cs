using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using sample.Shared.Interfaces.Entities;

namespace sample.Shared.Interfaces.Repositories
{
    public interface IRepository<TEntity>
        where TEntity : class, IEntity
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(Guid id);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
