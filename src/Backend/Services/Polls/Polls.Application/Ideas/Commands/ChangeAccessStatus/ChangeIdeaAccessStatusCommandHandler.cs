using MediatR;
using Polls.Application.Common.Extensions;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;

namespace Polls.Application.Ideas.Commands.ChangeAccessStatus;

public sealed class ChangeIdeaAccessStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventPublisher eventPublisher,
    IDateTimeProvider dateTimeProvider) 
    : IRequestHandler<ChangeIdeaAccessStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        ChangeIdeaAccessStatusCommand command, 
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetByIdAsync(command.Id, cancellationToken);
        if (idea is null)
            return IdeaErrors.NotFound(command.Id);

        if (command.NewIdeaAccessStatus == IdeaAccessStatus.Undefined)
            return IdeaErrors.InvalidStatus(command.NewIdeaAccessStatus);
        
        if (idea.AccessStatus == command.NewIdeaAccessStatus)
            return Result<Unit>.Success(Unit.Value);

        var poll = await unitOfWork.Polls.GetByIdAsync(idea.PollId, cancellationToken);
        if (poll is null)
            return PollErrors.NotFound(idea.PollId);
        
        idea.AccessStatus = command.NewIdeaAccessStatus;
        unitOfWork.Ideas.Update(idea);

        await eventPublisher.PublishAsync(
            idea.ToAccessStatusChangedEvent(poll, dateTimeProvider.UtcNow),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}
