using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;

namespace Polls.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(ApplicationDbContext context) : IRepository<TEntity> where TEntity : EntityBase
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    
    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public Task CreateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
