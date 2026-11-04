using MediatR;
using Polls.Application.Common.Extensions;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;

namespace Polls.Application.Ideas.Commands.ChangeApprovalStatus;

public sealed class ChangeIdeaApprovalStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventPublisher eventPublisher,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<ChangeIdeaApprovalStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        ChangeIdeaApprovalStatusCommand command,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetByIdAsync(command.Id, cancellationToken);

        if (idea is null)
            return IdeaErrors.NotFound(command.Id);

        if (command.NewApprovalStatus == IdeaApprovalStatus.Undefined)
            return IdeaErrors.InvalidStatus(command.NewApprovalStatus);
        
        if (idea.ApprovalStatus == command.NewApprovalStatus)
            return Result<Unit>.Success(Unit.Value);

        var poll = await unitOfWork.Polls.GetByIdAsync(idea.PollId, cancellationToken);
        if (poll is null)
            return PollErrors.NotFound(idea.PollId);

        idea.ApprovalStatus = command.NewApprovalStatus;
        unitOfWork.Ideas.Update(idea);

        await eventPublisher.PublishAsync(
            idea.ToApprovalStatusChangedEvent(poll, dateTimeProvider.UtcNow),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}
