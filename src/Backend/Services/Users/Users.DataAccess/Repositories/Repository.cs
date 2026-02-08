using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Users.DataAccess.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : EntityBase
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();
    
    public async Task<T?> GetByIdAsync(Guid id ,CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(T entity)
    {
        _dbSet.Add(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
