using System.Linq.Expressions;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Common;
using Electium.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Electium.Persistence.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    private readonly ElectiumDbContext _context;
    private readonly DbSet<TEntity> _set;

    public GenericRepository(ElectiumDbContext context)
    {
        _context = context;
        _set = context.Set<TEntity>();
    }

    public async Task<List<TEntity>> GetAllAsync(params string[] includes)
        => await WithIncludes(includes).OrderBy(e => e.Id).ToListAsync();

    public async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, params string[] includes)
        => await WithIncludes(includes).Where(predicate).OrderBy(e => e.Id).ToListAsync();

    public async Task<TEntity?> GetByIdAsync(int id, params string[] includes)
        => await WithIncludes(includes).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        => await _set.AnyAsync(predicate);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        => predicate is null ? await _set.CountAsync() : await _set.CountAsync(predicate);

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await _set.AddAsync(entity);
        await SaveAsync("No se pudo guardar: el registro está duplicado o tiene datos relacionados inválidos.");
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _set.AddRangeAsync(entities);
        await SaveAsync("No se pudo guardar: el registro está duplicado o tiene datos relacionados inválidos.");
    }

    public async Task UpdateAsync(TEntity entity)
    {
        _set.Update(entity);
        await SaveAsync("No se pudo actualizar: los datos entran en conflicto con otro registro.");
    }

    public async Task DeleteAsync(TEntity entity)
    {
        _set.Remove(entity);
        await SaveAsync("No se puede eliminar este registro porque tiene información relacionada.");
    }

    private IQueryable<TEntity> WithIncludes(string[] includes)
    {
        IQueryable<TEntity> query = _set;
        foreach (var include in includes)
            query = query.Include(include);

        return query;
    }

    private async Task SaveAsync(string friendlyMessage)
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Quita lo pendiente para que el DbContext (scoped) no quede en mal estado.
            _context.ChangeTracker.Clear();
            throw new BusinessException(friendlyMessage);
        }
    }
}
