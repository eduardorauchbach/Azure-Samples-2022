using RauchTech.Extensions.Data.Cosmos.Models;
using System.Linq.Expressions;

namespace RauchTech.Extensions.Data.Cosmos
{
    public interface IBaseRepository<TEntity> where TEntity : EntityRoot
    {
        Task AddAsync(TEntity entities);

        Task UpdateAsync(TEntity entities);

        Task DeleteAsync(TEntity entities);


        Task BulkInsertAsync(IEnumerable<TEntity> entities);

        Task BulkUpdateAsync(IEnumerable<TEntity> entities);

        Task BulkDeleteAsync(IEnumerable<TEntity> entities);


        Task<TEntity?> GetByIdAsync(Guid id);

        Task<TEntity?> GetAsync(TEntity entity);

        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>>? predicate = null);

        Task<ICollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null);

        Task<ICollection<TReturn>> GetAllAsync<TReturn>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TReturn>> selector);

        Task<bool> Any(Expression<Func<TEntity, bool>> predicate);
    }
}
