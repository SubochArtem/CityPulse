using MediatR;
using Microsoft.Extensions.Logging; 
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Cities.Commands.ChangeStatus;

public sealed class ChangeCityStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ILogger<ChangeCityStatusCommandHandler> logger) 
    : IRequestHandler<ChangeCityStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        ChangeCityStatusCommand command, 
        CancellationToken cancellationToken)
    {
        var city = await unitOfWork.Cities.GetByIdAsync(command.Id, cancellationToken);
        if (city is null)
            return CityErrors.NotFound(command.Id);

        if (city.Status == command.NewStatus)
            return Result<Unit>.Success(Unit.Value);

        var (sourcePollStatus, targetPollStatus, sourceIdeaAccessStatus, targetIdeaAccessStatus) = GetStatusTransition(command.NewStatus);
        
        if (sourcePollStatus == PollStatus.Undefined || targetPollStatus == PollStatus.Undefined
            || sourceIdeaAccessStatus == IdeaAccessStatus.Undefined || targetIdeaAccessStatus == IdeaAccessStatus.Undefined)
        {
            logger.LogWarning(
                "Unsupported city status transition {CityId}: {Status}", 
                command.Id, 
                command.NewStatus);
            return CityErrors.InvalidStatus(command.NewStatus);
        }

        var utcNow = dateTimeProvider.UtcNow;
        
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            city.Status = command.NewStatus;
            unitOfWork.Cities.Update(city);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            await unitOfWork.Polls.UpdateStatusByCityAsync(
                city.Id, 
                sourcePollStatus, 
                targetPollStatus, 
                utcNow, 
                cancellationToken);
                
            await unitOfWork.Ideas.UpdateAccessStatusByCityAsync(
                city.Id, 
                sourceIdeaAccessStatus, 
                targetIdeaAccessStatus, 
                utcNow, 
                cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, 
                "Error changing city {CityId} status to {Status}", 
                command.Id, 
                command.NewStatus);

            await transaction.RollbackAsync(cancellationToken);
            return CommonErrors.DatabaseError;
        }
    }

    private static (
        PollStatus SourcePollStatus, 
        PollStatus TargetPollStatus, 
        IdeaAccessStatus SourceIdeaAccessStatus, 
        IdeaAccessStatus TargetIdeaAccessStatus)
        GetStatusTransition(CityStatus newStatus) => newStatus switch
    {
        CityStatus.Active => (
            PollStatus.Suspended, PollStatus.Active,
            IdeaAccessStatus.RestrictedByContext, IdeaAccessStatus.Active),
            
        CityStatus.Inactive => (
            PollStatus.Active, PollStatus.Suspended,
            IdeaAccessStatus.Active, IdeaAccessStatus.RestrictedByContext),
        
        _ => (PollStatus.Undefined, PollStatus.Undefined, 
            IdeaAccessStatus.Undefined, IdeaAccessStatus.Undefined)
    };
}
