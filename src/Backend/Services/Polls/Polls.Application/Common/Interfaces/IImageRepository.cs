using Polls.Domain.Images;

namespace Polls.Application.Common.Interfaces;

public interface IImageRepository<TImage> where TImage : Image
{
    Task<IReadOnlyList<TImage>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    void AddRange(IEnumerable<TImage> images);
    void RemoveRange(IEnumerable<TImage> images);
}
