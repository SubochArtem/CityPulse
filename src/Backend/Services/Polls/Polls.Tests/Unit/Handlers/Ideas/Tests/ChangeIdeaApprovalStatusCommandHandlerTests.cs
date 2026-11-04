using CityPulse.Contracts.Events.Polls;
using CityPulse.Contracts.Events.Polls.Enums;
using Moq;
using Polls.Application.Ideas.Commands.ChangeApprovalStatus;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Handlers.Ideas.Tests;

public sealed class ChangeIdeaApprovalStatusCommandHandlerTests
    : ChangeIdeaStatusHandlerTestsBase<ChangeIdeaApprovalStatusCommand, IdeaApprovalStatus>
{
    private readonly ChangeIdeaApprovalStatusCommandHandler _sut;

    public ChangeIdeaApprovalStatusCommandHandlerTests()
    {
        _sut = new ChangeIdeaApprovalStatusCommandHandler(UnitOfWork.Object, EventPublisher.Object, Clock.Object);
    }

    protected override ChangeIdeaApprovalStatusCommand Build(Guid id, IdeaApprovalStatus status) => new(id, status);

    protected override IdeaApprovalStatus GetStatus(Idea idea) => idea.ApprovalStatus;

    protected override void SetStatus(Idea idea, IdeaApprovalStatus status) => idea.ApprovalStatus = status;

    protected override Error ExpectedInvalidStatusError(IdeaApprovalStatus status) => IdeaErrors.InvalidStatus(status);

    protected override Task<Result<MediatRUnit>> HandleAsync(
        ChangeIdeaApprovalStatusCommand command,
        CancellationToken cancellationToken) =>
        _sut.Handle(command, cancellationToken);

    [Fact]
    public async Task Handle_StatusIsDifferent_PublishesStatusChangedEvent()
    {
        var idea = SetupIdeaWithStatus(IdeaApprovalStatus.Pending);
        var poll = SetupPollFor(idea);

        await ChangeStatusAsync(idea, IdeaApprovalStatus.Approved);

        EventPublisher.Verify(
            x => x.PublishAsync(
                It.Is<IdeaApprovalStatusChangedEvent>(e =>
                    e.IdeaId == idea.Id &&
                    e.PollId == poll.Id &&
                    e.CityId == poll.CityId &&
                    e.ApprovalStatus == IdeaReviewStatus.Approved &&
                    e.OccurredAt == Now),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
