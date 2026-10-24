using AutoMapper;
using Moq;
using Polls.Application.Common.Interfaces;
using Polls.Application.Polls.DTOs;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Unit.Mapping.Tests;

public sealed class PollMappingProfileTests
{
    private const string UrlPrefix = "https://storage.example.com/";
    private const int IdeasCount = 2;

    private readonly Mock<IImageStorageService> _storage = new(MockBehavior.Strict);
    private readonly IMapper _mapper;

    public PollMappingProfileTests()
    {
        _storage
            .Setup(x => x.GetUrl(It.IsAny<string>()))
            .Returns<string>(fileName => UrlPrefix + fileName);

        _mapper = TestMapper.Create(_storage.Object);
    }

    [Fact]
    public void Map_PollToPollDto_MapsScalarFields()
    {
        var poll = PollFakers.Polls().Generate();

        var dto = _mapper.Map<PollDto>(poll);

        Assert.Equal(poll.Id, dto.Id);
        Assert.Equal(poll.Title, dto.Title);
        Assert.Equal(poll.Description, dto.Description);
        Assert.Equal(poll.CityId, dto.CityId);
        Assert.Equal(poll.EndsAt, dto.EndsAt);
        Assert.Equal(poll.BudgetAmount, dto.BudgetAmount);
    }

    [Fact]
    public void Map_PollToPollDto_MapsTypeAndStatusAsNumbers()
    {
        var poll = PollFakers.Polls().Generate();

        var dto = _mapper.Map<PollDto>(poll);

        Assert.Equal((int)poll.Type, dto.Type);
        Assert.Equal((int)poll.Status, dto.Status);
    }

    [Fact]
    public void Map_PollToPollDto_MapsImagesWithUrlsFromStorage()
    {
        var poll = PollFakers.PollWithImages("first.jpg", "second.png");

        var dto = _mapper.Map<PollDto>(poll);

        Assert.Equal(poll.Images.Select(i => i.Id), dto.Images.Select(i => i.Id));
        Assert.Equal(poll.Images.Select(i => i.Order), dto.Images.Select(i => i.Order));
        Assert.Equal(poll.Images.Select(i => UrlPrefix + i.FileName), dto.Images.Select(i => i.Url));
    }

    [Fact]
    public void Map_PollWithoutImagesToPollDto_ReturnsEmptyImages()
    {
        var poll = PollFakers.Polls().Generate();

        var dto = _mapper.Map<PollDto>(poll);

        Assert.Empty(dto.Images);
    }

    [Fact]
    public void Map_PollToPollWithIdeasDto_MapsIdeasAndImages()
    {
        var poll = PollFakers.PollWithImages("first.jpg");
        poll.Ideas = IdeaFakers.Ideas().Generate(IdeasCount);

        var dto = _mapper.Map<PollWithIdeasDto>(poll);

        Assert.Equal(poll.Id, dto.Id);
        Assert.Equal(poll.Title, dto.Title);
        Assert.Equal(poll.Ideas.Select(i => i.Id), dto.Ideas.Select(i => i.Id));
        Assert.Equal([UrlPrefix + "first.jpg"], dto.Images.Select(i => i.Url));
    }
}
