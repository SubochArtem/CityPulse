using Microsoft.EntityFrameworkCore;
using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;

namespace Users.DataAccess.Repositories;

public class Repository<TEntity>(ApplicationDbContext context) : IRepository<TEntity> where TEntity : EntityBase
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
