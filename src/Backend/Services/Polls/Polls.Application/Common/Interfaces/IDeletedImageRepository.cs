using Polls.Domain.Images;

namespace Polls.Application.Common.Interfaces;

public interface IDeletedImageRepository
{
    Task<IReadOnlyList<DeletedImage>> GetBatchAsync(
        int batchSize, 
        CancellationToken cancellationToken = default);
    
    void AddRange(IEnumerable<DeletedImage> images);
    void RemoveRange(IEnumerable<DeletedImage> images);
}
