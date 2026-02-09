using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Users.DataAccess.Repositories;

public class Repository<TEntity>(ApplicationDbContext context) : IRepository<TEntity> where TEntity : EntityBase
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    
    public async Task<TEntity?> GetByIdAsync(Guid id ,CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public Task CreateAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        
        return Task.CompletedTask;
    }

    public  Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
