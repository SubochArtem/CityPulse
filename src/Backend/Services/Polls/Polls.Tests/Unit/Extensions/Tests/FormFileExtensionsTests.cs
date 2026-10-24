using Microsoft.AspNetCore.Http;
using Moq;
using Polls.API.Common.Extensions;
using Xunit;

namespace Polls.Tests.Unit.Extensions.Tests;

public sealed class FormFileExtensionsTests
{
    private static (Mock<IFormFile> File, MemoryStream Stream) BuildFile(string fileName, string contentType)
    {
        var stream = new MemoryStream([1, 2, 3]);
        var file = new Mock<IFormFile>(MockBehavior.Strict);
        file.Setup(x => x.FileName).Returns(fileName);
        file.Setup(x => x.ContentType).Returns(contentType);
        file.Setup(x => x.OpenReadStream()).Returns(stream);

        return (file, stream);
    }

    [Fact]
    public void ToImageFiles_FilesAreNull_ReturnsEmptyList()
    {
        IEnumerable<IFormFile>? files = null;

        var result = files.ToImageFiles();

        Assert.Empty(result);
    }

    [Fact]
    public void ToImageFiles_NoFiles_ReturnsEmptyList()
    {
        var result = Array.Empty<IFormFile>().ToImageFiles();

        Assert.Empty(result);
    }

    [Fact]
    public void ToImageFiles_FilesProvided_MapsNameTypeAndStreamKeepingOrder()
    {
        var (first, firstStream) = BuildFile("first.jpg", "image/jpeg");
        var (second, secondStream) = BuildFile("second.png", "image/png");

        var result = new[] { first.Object, second.Object }.ToImageFiles();

        Assert.Equal(["first.jpg", "second.png"], result.Select(f => f.FileName));
        Assert.Equal(["image/jpeg", "image/png"], result.Select(f => f.ContentType));
        Assert.Same(firstStream, result[0].Content);
        Assert.Same(secondStream, result[1].Content);
    }

    [Fact]
    public void ToImageFiles_FilesProvided_OpensEachStreamOnce()
    {
        var (file, _) = BuildFile("photo.jpg", "image/jpeg");

        _ = new[] { file.Object }.ToImageFiles();

        file.Verify(x => x.OpenReadStream(), Times.Once);
    }
}
