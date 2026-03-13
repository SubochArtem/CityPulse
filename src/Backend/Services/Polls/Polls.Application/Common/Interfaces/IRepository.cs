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

    void Create(TEntity entity,
        CancellationToken cancellationToken = default);

    void Update(TEntity entity,
        CancellationToken cancellationToken = default);

    void Delete(TEntity entity,
        CancellationToken cancellationToken = default);
}
