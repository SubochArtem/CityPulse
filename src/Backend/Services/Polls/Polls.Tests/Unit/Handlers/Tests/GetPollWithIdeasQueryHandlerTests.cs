using Moq;
using Polls.Application.Polls.Queries.GetPollWithIdeas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Guards.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Tests;

public sealed class GetPollWithIdeasQueryHandlerTests : HandlerTestsBase
{
    private const int IdeasCount = 2;

    private readonly GetPollWithIdeasQueryHandler _sut;

    public GetPollWithIdeasQueryHandlerTests()
    {
        _sut = new GetPollWithIdeasQueryHandler(UnitOfWork.Object, Mapper);
    }

    private void SetupPoll(Poll? poll, Guid id, IdeaAccessStatus? expectedIdeaFilter) =>
        UnitOfWork.Polls
            .Setup(x => x.GetWithIdeasAsync(id, expectedIdeaFilter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

    private static Poll BuildPollWithIdeas()
    {
        var poll = PollFakers.Polls().Generate();
        poll.Ideas = IdeaFakers.Ideas().Generate(IdeasCount);

        return poll;
    }

    [Fact]
    public async Task Handle_OnlyActiveRequested_AsksRepositoryForActiveIdeasOnly()
    {
        var poll = BuildPollWithIdeas();
        SetupPoll(poll, poll.Id, IdeaAccessStatus.Active);

        await _sut.Handle(new GetPollWithIdeasQuery(poll.Id, IncludeOnlyActive: true), CancellationToken.None);

        UnitOfWork.Polls.Verify(
            x => x.GetWithIdeasAsync(poll.Id, IdeaAccessStatus.Active, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AllRequested_AsksRepositoryForIdeasOfAnyAccessStatus()
    {
        var poll = BuildPollWithIdeas();
        SetupPoll(poll, poll.Id, null);

        await _sut.Handle(new GetPollWithIdeasQuery(poll.Id, IncludeOnlyActive: false), CancellationToken.None);

        UnitOfWork.Polls.Verify(
            x => x.GetWithIdeasAsync(poll.Id, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PollFound_ReturnsMappedPollWithIdeas()
    {
        var poll = BuildPollWithIdeas();
        SetupPoll(poll, poll.Id, IdeaAccessStatus.Active);

        var result = await _sut.Handle(new GetPollWithIdeasQuery(poll.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(poll.Id, result.Value!.Id);
        Assert.Equal(poll.Ideas.Select(i => i.Id), result.Value.Ideas.Select(i => i.Id));
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFoundError()
    {
        var id = Guid.NewGuid();
        SetupPoll(null, id, IdeaAccessStatus.Active);

        var result = await _sut.Handle(new GetPollWithIdeasQuery(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound(id), result.Error);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.NotActiveStatuses), MemberType = typeof(PollGuardTestData))]
    public async Task Handle_PollIsNotActiveAndOnlyActiveRequested_ReturnsNotFoundError(PollStatus status)
    {
        var poll = BuildPollWithIdeas();
        poll.Status = status;
        SetupPoll(poll, poll.Id, IdeaAccessStatus.Active);

        var result = await _sut.Handle(
            new GetPollWithIdeasQuery(poll.Id, IncludeOnlyActive: true),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound(poll.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.NotActiveStatuses), MemberType = typeof(PollGuardTestData))]
    public async Task Handle_PollIsNotActiveAndAllRequested_ReturnsPollWithIdeas(PollStatus status)
    {
        var poll = BuildPollWithIdeas();
        poll.Status = status;
        SetupPoll(poll, poll.Id, null);

        var result = await _sut.Handle(
            new GetPollWithIdeasQuery(poll.Id, IncludeOnlyActive: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((int)status, result.Value!.Status);
        Assert.Equal(IdeasCount, result.Value.Ideas.Count);
    }
}
