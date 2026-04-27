using MediatR;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Domain.Common;
using Polls.Domain.Images;

namespace Polls.Application.Images.Validators;

public static class ImageValidator
{
    private static readonly Dictionary<string, byte[]> MagicNumbers = new()
    {
        { "jpg", [0xFF, 0xD8, 0xFF] },
        { "jpeg", [0xFF, 0xD8, 0xFF] },
        { "png", [0x89, 0x50, 0x4E, 0x47] },
        { "webp", [0x52, 0x49, 0x46, 0x46] }
    };

    public static Result<Unit> Validate(
        IReadOnlyList<ImageFile> images,
        int currentCount = 0)
    {
        if (images.Count + currentCount > ValidationConstants.Image.MaxCount)
            return ImageErrors.MaxImagesExceeded(ValidationConstants.Image.MaxCount);

        foreach (var image in images)
        {
            var extension = Path.GetExtension(image.FileName)
                .TrimStart('.')
                .ToLower();

            if (!ValidationConstants.Image.AllowedFormats.Contains(extension))
                return ImageErrors.InvalidFormat(ValidationConstants.Image.AllowedFormats);

            if (image.Content.Length > ValidationConstants.Image.MaxFileSizeBytes)
                return ImageErrors.FileTooLarge(ValidationConstants.Image.MaxFileSizeMb);

            if (!IsValidSignature(image.Content, extension))
                return ImageErrors.InvalidFormat(ValidationConstants.Image.AllowedFormats);
        }

        return Unit.Value;
    }

    private static bool IsValidSignature(Stream stream, string extension)
    {
        if (!MagicNumbers.TryGetValue(extension, out var signature))
            return false;

        var buffer = new byte[signature.Length];
        var position = stream.Position;

        stream.ReadExactly(buffer, 0, buffer.Length);
        stream.Position = position;

        return buffer.SequenceEqual(signature);
    }
}
