using Polls.Domain.Common;

namespace Polls.Domain.Images;

public static class ImageErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound($"Image with id '{id}' was not found");

    public static Error NotBelongsToEntity(Guid id) =>
        Error.Conflict($"Image with id '{id}' does not belong to this entity");

    public static Error MaxImagesExceeded(int maxImagesCount) =>
        Error.Conflict($"Maximum number of images ({maxImagesCount}) exceeded");

    public static Error InvalidFormat(string[] allowedFormats) =>
        Error.Conflict($"Invalid image format. Allowed: {string.Join(", ", allowedFormats)}");

    public static  Error FileTooLarge(int maxImageSize) =>
        Error.Conflict($"Image size exceeds {maxImageSize}MB limit");
}
