using Moq;
using Polls.Application.Ideas.Queries.GetIdeaCity;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Ideas.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Ideas.Tests;

public sealed class GetIdeaCityQueryHandlerTests : HandlerTestsBase
{
    private readonly GetIdeaCityQueryHandler _sut;

    public GetIdeaCityQueryHandlerTests()
    {
        _sut = new GetIdeaCityQueryHandler(UnitOfWork.Object);
    }

    private void SetupIdea(Idea? idea, Guid id) =>
        UnitOfWork.Ideas
            .Setup(x => x.GetWithPollAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(idea);

    [Fact]
    public async Task Handle_IdeaIsActive_ReturnsIdeaIdAndCityOfItsPoll()
    {
        var idea = IdeaFakers.IdeaWithPoll();
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(new GetIdeaCityQuery(idea.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(idea.Id, result.Value!.IdeaId);
        Assert.Equal(idea.Poll.CityId, result.Value.CityId);
    }

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFoundError()
    {
        var id = Guid.NewGuid();
        SetupIdea(null, id);

        var result = await _sut.Handle(new GetIdeaCityQuery(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotFound(id), result.Error);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.NotActiveStatuses), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_IdeaIsNotActiveAndOnlyActiveRequested_ReturnsNotFoundError(IdeaAccessStatus status)
    {
        var idea = IdeaFakers.IdeaWithPoll();
        idea.AccessStatus = status;
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(new GetIdeaCityQuery(idea.Id, IncludeOnlyActive: true), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotFound(idea.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.NotActiveStatuses), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_IdeaIsNotActiveAndAllRequested_ReturnsCity(IdeaAccessStatus status)
    {
        var idea = IdeaFakers.IdeaWithPoll();
        idea.AccessStatus = status;
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(new GetIdeaCityQuery(idea.Id, IncludeOnlyActive: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(idea.Poll.CityId, result.Value!.CityId);
    }
}
