using MediatR;
using Microsoft.Extensions.Logging;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Commands.ChangeStatus;

public sealed class ChangePollStatusCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<ChangePollStatusCommandHandler> logger) 
    : IRequestHandler<ChangePollStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        ChangePollStatusCommand command, 
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetByIdAsync(command.Id, cancellationToken);
        if (poll is null)
            return PollErrors.NotFound(command.Id);
        
        if (poll.Status == command.NewStatus)
            return Result<Unit>.Success(Unit.Value);
        
        var transition = GetIdeaStatusTransition(command.NewStatus);
        if (transition is null)
        {
            logger.LogWarning("Unsupported poll status {PollId}: {Status}", command.Id, command.NewStatus);
            return PollErrors.InvalidStatus(command.NewStatus);
        }

        var utcNow = DateTimeOffset.UtcNow;
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            poll.Status = command.NewStatus;
            unitOfWork.Polls.Update(poll);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            await unitOfWork.Ideas.UpdateStatusByPollIdAsync(
                poll.Id, 
                transition.Value.Source,
                transition.Value.Target,
                utcNow, 
                cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing poll {PollId} status to {Status}", command.Id, command.NewStatus);
            await transaction.RollbackAsync(cancellationToken);
            return CommonErrors.DatabaseError;
        }
    }

    private static (IdeaStatus Source, IdeaStatus Target)? GetIdeaStatusTransition(
        PollStatus newStatus) =>
        newStatus switch
        {
            PollStatus.Active => (IdeaStatus.Suspended, IdeaStatus.Active),
            PollStatus.Suspended => (IdeaStatus.Active, IdeaStatus.Suspended),
            _=> null
        };
}
