using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Users.DataAccess.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : EntityBase
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();
    
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
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

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
