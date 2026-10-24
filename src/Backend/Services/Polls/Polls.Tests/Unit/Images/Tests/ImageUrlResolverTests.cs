using Moq;
using Polls.Application.Common.Interfaces;
using Polls.Application.Images.DTOs;
using Polls.Application.Images.Resolvers;
using Polls.Domain.Images;
using Xunit;

namespace Polls.Tests.Unit.Images.Tests;

public sealed class ImageUrlResolverTests
{
    private const string FirstFileName = "first.jpg";
    private const string SecondFileName = "second.png";
    private const string BaseUrl = "https://storage.example.com/polls-images/";

    private readonly Mock<IImageStorageService> _storage = new(MockBehavior.Strict);
    private readonly ImageUrlResolver _sut;

    public ImageUrlResolverTests()
    {
        _sut = new ImageUrlResolver(_storage.Object);
    }

    private string Resolve(Image source) =>
        _sut.Resolve(source, new ImageDto { Url = string.Empty }, string.Empty, null!);

    [Fact]
    public void Resolve_ImageHasFileName_ReturnsUrlFromStorage()
    {
        _storage.Setup(x => x.GetUrl(FirstFileName)).Returns(BaseUrl + FirstFileName);

        var url = Resolve(new Image { FileName = FirstFileName });

        Assert.Equal(BaseUrl + FirstFileName, url);
        _storage.Verify(x => x.GetUrl(FirstFileName), Times.Once);
    }

    [Fact]
    public void Resolve_ImagesHaveDifferentFileNames_ReturnsDifferentUrls()
    {
        _storage.Setup(x => x.GetUrl(FirstFileName)).Returns(BaseUrl + FirstFileName);
        _storage.Setup(x => x.GetUrl(SecondFileName)).Returns(BaseUrl + SecondFileName);

        var first = Resolve(new Image { FileName = FirstFileName });
        var second = Resolve(new Image { FileName = SecondFileName });

        Assert.NotEqual(first, second);
        Assert.Equal(BaseUrl + SecondFileName, second);
    }
}
