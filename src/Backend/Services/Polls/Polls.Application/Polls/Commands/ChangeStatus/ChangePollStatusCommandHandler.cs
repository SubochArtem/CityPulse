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
    IDateTimeProvider dateTimeProvider,
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
        
        var (sourceStatus, targetStatus) = GetIdeaStatusTransition(command.NewStatus);
        
        if (sourceStatus == IdeaAccessStatus.Undefined || targetStatus == IdeaAccessStatus.Undefined)
        {
            logger.LogWarning("Unsupported poll status transition for {PollId}: {Status}", 
                command.Id, command.NewStatus);
            return PollErrors.InvalidStatus(command.NewStatus);
        }

        var utcNow = dateTimeProvider.UtcNow;
        
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            poll.Status = command.NewStatus;
            unitOfWork.Polls.Update(poll);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            await unitOfWork.Ideas.UpdateAccessStatusByPollIdAsync(
                poll.Id, 
                sourceStatus,
                targetStatus,
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

    private static (IdeaAccessStatus Source, IdeaAccessStatus Target) GetIdeaStatusTransition(
        PollStatus newStatus) =>
        newStatus switch
        {
            PollStatus.Active => (IdeaAccessStatus.RestrictedByContext, IdeaAccessStatus.Active),
            PollStatus.Suspended => (IdeaAccessStatus.Active, IdeaAccessStatus.RestrictedByContext),
            _=> (IdeaAccessStatus.Undefined, IdeaAccessStatus.Undefined)
        };
}
