using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;

namespace Polls.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(ApplicationDbContext context) : IRepository<TEntity> where TEntity : EntityBase
{
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
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public void Create(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        _dbSet.Add(entity);
    }

    public void Update(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
    }

    public void Delete(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
    }
}
