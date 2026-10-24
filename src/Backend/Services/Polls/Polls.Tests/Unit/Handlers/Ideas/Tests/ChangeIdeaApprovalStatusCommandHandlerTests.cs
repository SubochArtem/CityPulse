using Polls.Application.Ideas.Commands.ChangeApprovalStatus;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Handlers.Ideas.Tests;

public sealed class ChangeIdeaApprovalStatusCommandHandlerTests
    : ChangeIdeaStatusHandlerTestsBase<ChangeIdeaApprovalStatusCommand, IdeaApprovalStatus>
{
    private readonly ChangeIdeaApprovalStatusCommandHandler _sut;

    public ChangeIdeaApprovalStatusCommandHandlerTests()
    {
        _sut = new ChangeIdeaApprovalStatusCommandHandler(UnitOfWork.Object);
    }

    protected override ChangeIdeaApprovalStatusCommand Build(Guid id, IdeaApprovalStatus status) => new(id, status);

    protected override IdeaApprovalStatus GetStatus(Idea idea) => idea.ApprovalStatus;

    protected override void SetStatus(Idea idea, IdeaApprovalStatus status) => idea.ApprovalStatus = status;

    protected override Task<Result<MediatRUnit>> HandleAsync(
        ChangeIdeaApprovalStatusCommand command,
        CancellationToken cancellationToken) =>
        _sut.Handle(command, cancellationToken);
}
