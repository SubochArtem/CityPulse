using Polls.Application.Common.Models;

namespace Polls.API.Common.Extensions;

public static class FormFileExtensions
{
    public static IReadOnlyList<ImageFile> ToImageFiles(this IEnumerable<IFormFile>? files)
    {
        return files?
            .Select(f => new ImageFile 
            { 
                FileName = f.FileName, 
                Content = f.OpenReadStream(), 
                ContentType = f.ContentType 
            })
            .ToList() ?? [];
    }
}
