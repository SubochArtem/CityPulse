using AutoMapper;
using Moq;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.DTOs;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Unit.Mapping.Tests;

public sealed class IdeaMappingProfileTests
{
    private const string UrlPrefix = "https://storage.example.com/";

    private readonly Mock<IImageStorageService> _storage = new(MockBehavior.Strict);
    private readonly IMapper _mapper;

    public IdeaMappingProfileTests()
    {
        _storage
            .Setup(x => x.GetUrl(It.IsAny<string>()))
            .Returns<string>(fileName => UrlPrefix + fileName);

        _mapper = TestMapper.Create(_storage.Object);
    }

    [Fact]
    public void Map_IdeaToIdeaDto_MapsScalarFields()
    {
        var idea = IdeaFakers.IdeaWithPoll();

        var dto = _mapper.Map<IdeaDto>(idea);

        Assert.Equal(idea.Id, dto.Id);
        Assert.Equal(idea.UserId, dto.UserId);
        Assert.Equal(idea.PollId, dto.PollId);
        Assert.Equal(idea.Title, dto.Title);
        Assert.Equal(idea.Description, dto.Description);
        Assert.Equal((int)idea.AccessStatus, dto.AccessStatus);
        Assert.Equal((int)idea.ApprovalStatus, dto.ApprovalStatus);
    }

    [Fact]
    public void Map_IdeaToIdeaDto_MapsImagesWithUrlsFromStorage()
    {
        var idea = IdeaFakers.IdeaWithImages("first.jpg", "second.png");

        var dto = _mapper.Map<IdeaDto>(idea);

        Assert.Equal(idea.Images.Select(i => i.Id), dto.Images.Select(i => i.Id));
        Assert.Equal(idea.Images.Select(i => i.Order), dto.Images.Select(i => i.Order));
        Assert.Equal(idea.Images.Select(i => UrlPrefix + i.FileName), dto.Images.Select(i => i.Url));
    }

    [Fact]
    public void Map_IdeaWithImagesStoredOutOfOrderToIdeaDto_ReturnsImagesSortedByOrder()
    {
        var idea = IdeaFakers.IdeaWithImages("first.jpg", "second.png");
        idea.Images = idea.Images.Reverse().ToList();

        var dto = _mapper.Map<IdeaDto>(idea);

        Assert.Equal(idea.Images.OrderBy(i => i.Order).Select(i => i.Id), dto.Images.Select(i => i.Id));
    }

    [Fact]
    public void Map_IdeaWithImagesStoredOutOfOrderToIdeaWithPollDto_ReturnsImagesSortedByOrder()
    {
        var idea = IdeaFakers.IdeaWithImages("first.jpg", "second.png");
        idea.Images = idea.Images.Reverse().ToList();

        var dto = _mapper.Map<IdeaWithPollDto>(idea);

        Assert.Equal(idea.Images.OrderBy(i => i.Order).Select(i => i.Id), dto.Images.Select(i => i.Id));
    }

    [Fact]
    public void Map_IdeaWithoutImagesToIdeaDto_ReturnsEmptyImages()
    {
        var idea = IdeaFakers.IdeaWithPoll();

        var dto = _mapper.Map<IdeaDto>(idea);

        Assert.Empty(dto.Images);
    }

    [Fact]
    public void Map_IdeaToIdeaWithPollDto_MapsPollAndImages()
    {
        var idea = IdeaFakers.IdeaWithImages("first.jpg");

        var dto = _mapper.Map<IdeaWithPollDto>(idea);

        Assert.Equal(idea.Id, dto.Id);
        Assert.Equal(idea.Poll.Id, dto.Poll.Id);
        Assert.Equal(idea.Poll.Title, dto.Poll.Title);
        Assert.Equal(idea.Poll.CityId, dto.Poll.CityId);
        Assert.Equal([UrlPrefix + "first.jpg"], dto.Images.Select(i => i.Url));
    }
}
