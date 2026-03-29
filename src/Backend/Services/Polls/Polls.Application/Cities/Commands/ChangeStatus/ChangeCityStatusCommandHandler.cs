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
    ILogger<ChangeCityStatusCommandHandler> logger) 
    : IRequestHandler<ChangeCityStatusCommand, Result<Unit>>
{
    private const string UnsupportedTransitionErrorMessage = "Unsupported status transition.";

    public async Task<Result<Unit>> Handle(
        ChangeCityStatusCommand command, 
        CancellationToken cancellationToken)
    {
        var city = await unitOfWork.Cities.GetByIdAsync(command.Id, cancellationToken);
        if (city is null)
            return CityErrors.NotFound(command.Id);

        if (city.Status == command.NewStatus)
            return Unit.Value;

        var (sPoll, tPoll, sIdea, tIdea) = GetStatusMap(command.NewStatus);
        var utcNow = DateTimeOffset.UtcNow;

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            city.Status = command.NewStatus;
            unitOfWork.Cities.Update(city);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            await unitOfWork.Polls.UpdateStatusByCityAsync(
                city.Id, sPoll, tPoll, utcNow, cancellationToken);
                
            await unitOfWork.Ideas.UpdateStatusByCityAsync(
                city.Id, sIdea, tIdea, utcNow, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            
            return Unit.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, 
                "An error occurred while changing status for City {CityId} to {NewStatus}", 
                command.Id, 
                command.NewStatus);

            await transaction.RollbackAsync(cancellationToken);
            
            return CommonErrors.DatabaseError;
        }
    }

    private static (PollStatus sPoll, PollStatus tPoll, IdeaStatus sIdea, IdeaStatus tIdea) 
        GetStatusMap(CityStatus newStatus) => newStatus switch
    {
        CityStatus.Active => (
            PollStatus.Suspended, PollStatus.Active, 
            IdeaStatus.Suspended, IdeaStatus.Active),
            
        CityStatus.Inactive => (
            PollStatus.Active, PollStatus.Suspended, 
            IdeaStatus.Active, IdeaStatus.Suspended),
        
        _ => throw new ArgumentOutOfRangeException(nameof(newStatus), UnsupportedTransitionErrorMessage)
    };
}
