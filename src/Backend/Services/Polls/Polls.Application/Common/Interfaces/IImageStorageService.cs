using Polls.Application.Common.Models;

namespace Polls.Application.Common.Interfaces;

public interface IImageStorageService
{
    Task<IReadOnlyList<string>> UploadRangeAsync(
        IReadOnlyList<ImageFile> files, 
        CancellationToken cancellationToken = default);

    Task DeleteByNamesAsync(
        IEnumerable<string> objectNames,
        CancellationToken cancellationToken = default);
    
    string GetUrl(string fileName);
}
