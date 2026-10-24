using Polls.Domain.Common.Enums;
using Polls.Domain.Images;
using Xunit;

namespace Polls.Tests.Unit.Errors.Tests;

public sealed class ImageErrorsTests
{
    private const int MaxImagesCount = 5;
    private const int MaxImageSize = 10;

    [Fact]
    public void NotFound_WithId_ReturnsNotFoundErrorMentioningId()
    {
        var id = Guid.NewGuid();

        var error = ImageErrors.NotFound(id);

        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Contains(id.ToString(), error.Description);
    }

    [Fact]
    public void NotBelongsToEntity_WithId_ReturnsConflictErrorMentioningId()
    {
        var id = Guid.NewGuid();

        var error = ImageErrors.NotBelongsToEntity(id);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(id.ToString(), error.Description);
    }

    [Fact]
    public void MaxImagesExceeded_WithLimit_ReturnsConflictErrorMentioningLimit()
    {
        var error = ImageErrors.MaxImagesExceeded(MaxImagesCount);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(MaxImagesCount.ToString(), error.Description);
    }

    [Fact]
    public void InvalidFormat_WithAllowedFormats_ListsFormatsSeparatedByComma()
    {
        string[] allowedFormats = ["image/jpeg", "image/png"];

        var error = ImageErrors.InvalidFormat(allowedFormats);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains("image/jpeg, image/png", error.Description);
    }

    [Fact]
    public void InvalidFormat_WithNoAllowedFormats_StillReturnsConflictError()
    {
        var error = ImageErrors.InvalidFormat([]);

        Assert.Equal(ErrorType.Conflict, error.Type);
    }

    [Fact]
    public void FileTooLarge_WithLimit_ReturnsConflictErrorMentioningLimit()
    {
        var error = ImageErrors.FileTooLarge(MaxImageSize);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(MaxImageSize.ToString(), error.Description);
    }
}
