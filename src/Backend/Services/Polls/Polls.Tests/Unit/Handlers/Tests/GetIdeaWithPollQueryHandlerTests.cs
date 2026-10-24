using Moq;
using Polls.Application.Ideas.Queries.GetIdeaWithPoll;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Tests;

public sealed class GetIdeaWithPollQueryHandlerTests : HandlerTestsBase
{
    private readonly GetIdeaWithPollQueryHandler _sut;

    public GetIdeaWithPollQueryHandlerTests()
    {
        _sut = new GetIdeaWithPollQueryHandler(UnitOfWork.Object, Mapper);
    }

    private void SetupIdea(Idea? idea, Guid id) =>
        UnitOfWork.Ideas
            .Setup(x => x.GetWithPollAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(idea);

    [Fact]
    public async Task Handle_IdeaIsActive_ReturnsMappedIdeaWithPoll()
    {
        var idea = IdeaFakers.IdeaWithPoll();
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(new GetIdeaWithPollQuery(idea.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(idea.Id, result.Value!.Id);
        Assert.Equal(idea.Poll.Id, result.Value.Poll.Id);
        Assert.Equal(idea.Poll.CityId, result.Value.Poll.CityId);
    }

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFoundError()
    {
        var id = Guid.NewGuid();
        SetupIdea(null, id);

        var result = await _sut.Handle(new GetIdeaWithPollQuery(id), CancellationToken.None);

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

        var result = await _sut.Handle(
            new GetIdeaWithPollQuery(idea.Id, IncludeOnlyActive: true),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotFound(idea.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.NotActiveStatuses), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_IdeaIsNotActiveAndAllRequested_ReturnsIdeaWithPoll(IdeaAccessStatus status)
    {
        var idea = IdeaFakers.IdeaWithPoll();
        idea.AccessStatus = status;
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(
            new GetIdeaWithPollQuery(idea.Id, IncludeOnlyActive: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((int)status, result.Value!.AccessStatus);
        Assert.Equal(idea.Poll.Id, result.Value.Poll.Id);
    }
}
