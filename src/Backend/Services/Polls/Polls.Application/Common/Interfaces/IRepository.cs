using System.Linq.Expressions;
using Polls.Application.Common.Models;
using Polls.Domain.Common;

namespace Polls.Application.Common.Interfaces;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    Task<TEntity?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<PagedList<TEntity>> GetAllPagedAsync(
        BaseFilter filter,
        CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    void Create(TEntity entity);

    void Update(TEntity entity);

    void Delete(TEntity entity);
}
