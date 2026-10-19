using System.Linq.Expressions;
using Electium.Domain.Common;

namespace Electium.Application.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<List<TEntity>> GetAllAsync(params string[] includes);

    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, params string[] includes);

    Task<TEntity?> GetByIdAsync(int id, params string[] includes);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

    Task<TEntity> AddAsync(TEntity entity);

    Task AddRangeAsync(IEnumerable<TEntity> entities);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);
}
