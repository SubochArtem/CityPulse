using Polls.Application.Ideas.Commands.ChangeAccessStatus;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Handlers.Ideas.Tests;

public sealed class ChangeIdeaAccessStatusCommandHandlerTests
    : ChangeIdeaStatusHandlerTestsBase<ChangeIdeaAccessStatusCommand, IdeaAccessStatus>
{
    private readonly ChangeIdeaAccessStatusCommandHandler _sut;

    public ChangeIdeaAccessStatusCommandHandlerTests()
    {
        _sut = new ChangeIdeaAccessStatusCommandHandler(UnitOfWork.Object);
    }

    protected override ChangeIdeaAccessStatusCommand Build(Guid id, IdeaAccessStatus status) => new(id, status);

    protected override IdeaAccessStatus GetStatus(Idea idea) => idea.AccessStatus;

    protected override void SetStatus(Idea idea, IdeaAccessStatus status) => idea.AccessStatus = status;

    protected override Task<Result<MediatRUnit>> HandleAsync(
        ChangeIdeaAccessStatusCommand command,
        CancellationToken cancellationToken) =>
        _sut.Handle(command, cancellationToken);
}
