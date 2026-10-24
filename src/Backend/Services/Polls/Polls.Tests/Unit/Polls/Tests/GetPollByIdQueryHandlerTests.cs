using Moq;
using Polls.Application.Polls.Queries.GetPollById;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Guards.TestData;
using Xunit;

namespace Polls.Tests.Unit.Polls.Tests;

public sealed class GetPollByIdQueryHandlerTests : HandlerTestsBase
{
    private readonly GetPollByIdQueryHandler _sut;

    public GetPollByIdQueryHandlerTests()
    {
        _sut = new GetPollByIdQueryHandler(UnitOfWork.Object, Mapper);
    }

    private void SetupPoll(Poll? poll, Guid id) =>
        UnitOfWork.Polls
            .Setup(x => x.GetByIdWithImagesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

    [Fact]
    public async Task Handle_PollIsActive_ReturnsMappedPoll()
    {
        var poll = PollFakers.Polls().Generate();
        SetupPoll(poll, poll.Id);

        var result = await _sut.Handle(new GetPollByIdQuery(poll.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(poll.Id, result.Value!.Id);
        Assert.Equal(poll.Title, result.Value.Title);
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFoundError()
    {
        var id = Guid.NewGuid();
        SetupPoll(null, id);

        var result = await _sut.Handle(new GetPollByIdQuery(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound(id), result.Error);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.NotActiveStatuses), MemberType = typeof(PollGuardTestData))]
    public async Task Handle_PollIsNotActiveAndOnlyActiveRequested_ReturnsNotFoundError(PollStatus status)
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = status;
        SetupPoll(poll, poll.Id);

        var result = await _sut.Handle(new GetPollByIdQuery(poll.Id, IncludeOnlyActive: true), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound(poll.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.NotActiveStatuses), MemberType = typeof(PollGuardTestData))]
    public async Task Handle_PollIsNotActiveAndAllRequested_ReturnsPoll(PollStatus status)
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = status;
        SetupPoll(poll, poll.Id);

        var result = await _sut.Handle(new GetPollByIdQuery(poll.Id, IncludeOnlyActive: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((int)status, result.Value!.Status);
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesItToRepository()
    {
        var poll = PollFakers.Polls().Generate();
        using var cts = new CancellationTokenSource();
        UnitOfWork.Polls
            .Setup(x => x.GetByIdWithImagesAsync(poll.Id, cts.Token))
            .ReturnsAsync(poll);

        await _sut.Handle(new GetPollByIdQuery(poll.Id), cts.Token);

        UnitOfWork.Polls.Verify(x => x.GetByIdWithImagesAsync(poll.Id, cts.Token), Times.Once);
    }
}
