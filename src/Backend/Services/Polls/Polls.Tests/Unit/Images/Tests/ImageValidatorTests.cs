using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Application.Images.Validators;
using Polls.Domain.Images;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Images.TestData;
using Xunit;

namespace Polls.Tests.Unit.Images.Tests;

public sealed class ImageValidatorTests
{
    private static ImageFile ValidImage(int index = 0) =>
        ImageFiles.Create($"photo{index}.jpg", ImageFiles.JpegSignature);

    private static List<ImageFile> ValidImages(int count) =>
        Enumerable.Range(0, count).Select(ValidImage).ToList();

    [Fact]
    public void Validate_NoImages_ReturnsSuccess()
    {
        var result = ImageValidator.Validate([]);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(ImageValidatorTestData.OverLimitCounts), MemberType = typeof(ImageValidatorTestData))]
    public void Validate_TotalCountExceedsLimit_ReturnsMaxImagesExceededError(int newCount, int currentCount)
    {
        var result = ImageValidator.Validate(ValidImages(newCount), currentCount);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.MaxImagesExceeded(ValidationConstants.Image.MaxCount), result.Error);
    }

    [Theory]
    [MemberData(nameof(ImageValidatorTestData.WithinLimitCounts), MemberType = typeof(ImageValidatorTestData))]
    public void Validate_TotalCountWithinLimit_ReturnsSuccess(int newCount, int currentCount)
    {
        var result = ImageValidator.Validate(ValidImages(newCount), currentCount);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(ImageValidatorTestData.ValidImages), MemberType = typeof(ImageValidatorTestData))]
    public void Validate_FormatAndSignatureAreValid_ReturnsSuccess(string fileName, byte[] signature)
    {
        var image = ImageFiles.Create(fileName, signature);

        var result = ImageValidator.Validate([image]);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(ImageValidatorTestData.UnsupportedFileNames), MemberType = typeof(ImageValidatorTestData))]
    public void Validate_ExtensionIsNotSupported_ReturnsInvalidFormatError(string fileName)
    {
        var image = ImageFiles.Create(fileName, ImageFiles.JpegSignature);

        var result = ImageValidator.Validate([image]);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.InvalidFormat(ValidationConstants.Image.AllowedFormats), result.Error);
    }

    [Theory]
    [MemberData(nameof(ImageValidatorTestData.MismatchedSignatures), MemberType = typeof(ImageValidatorTestData))]
    public void Validate_SignatureDoesNotMatchExtension_ReturnsInvalidFormatError(string fileName, byte[] signature)
    {
        var image = ImageFiles.Create(fileName, signature);

        var result = ImageValidator.Validate([image]);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.InvalidFormat(ValidationConstants.Image.AllowedFormats), result.Error);
    }

    [Theory]
    [MemberData(nameof(ImageValidatorTestData.TooShortFiles), MemberType = typeof(ImageValidatorTestData))]
    public void Validate_FileIsShorterThanSignature_ReturnsInvalidFormatError(string fileName, byte[] bytes)
    {
        var image = ImageFiles.CreateFromBytes(fileName, bytes);

        var result = ImageValidator.Validate([image]);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.InvalidFormat(ValidationConstants.Image.AllowedFormats), result.Error);
    }

    [Fact]
    public void Validate_FileSizeIsAtLimit_ReturnsSuccess()
    {
        var size = (int)ValidationConstants.Image.MaxFileSizeBytes;
        var image = ImageFiles.Create("big.jpg", ImageFiles.JpegSignature, size);

        var result = ImageValidator.Validate([image]);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_FileSizeExceedsLimit_ReturnsFileTooLargeError()
    {
        var size = (int)ValidationConstants.Image.MaxFileSizeBytes + 1;
        var image = ImageFiles.Create("big.jpg", ImageFiles.JpegSignature, size);

        var result = ImageValidator.Validate([image]);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.FileTooLarge(ValidationConstants.Image.MaxFileSizeMb), result.Error);
    }

    [Fact]
    public void Validate_SecondImageIsInvalid_ReturnsItsError()
    {
        var images = new List<ImageFile>
        {
            ValidImage(),
            ImageFiles.Create("notes.txt", ImageFiles.JpegSignature)
        };

        var result = ImageValidator.Validate(images);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.InvalidFormat(ValidationConstants.Image.AllowedFormats), result.Error);
    }

    [Fact]
    public void Validate_CountAndFormatAreBothInvalid_ReturnsCountError()
    {
        var images = Enumerable
            .Range(0, ValidationConstants.Image.MaxCount + 1)
            .Select(index => ImageFiles.Create($"notes{index}.txt", ImageFiles.JpegSignature))
            .ToList();

        var result = ImageValidator.Validate(images);

        Assert.Equal(ImageErrors.MaxImagesExceeded(ValidationConstants.Image.MaxCount), result.Error);
    }

    [Fact]
    public void Validate_ValidImage_RestoresStreamPosition()
    {
        var image = ValidImage();

        ImageValidator.Validate([image]);

        Assert.Equal(0, image.Content.Position);
    }
}
