using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Security;
using Polls.Application.Polls.Guards;
using Polls.Domain.Authorization;
using Polls.Domain.Common;
using Polls.Domain.Polls;

namespace Polls.Application.Polls.Commands.DeletePoll;

public sealed class DeletePollCommandHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContext,
    CityAccessPolicy cityAccessPolicy)
    : IRequestHandler<DeletePollCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        DeletePollCommand command,
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetByIdAsync(command.Id, cancellationToken);

        if (poll is null)
            return PollErrors.NotFound(command.Id);

        var canDeleteAny = userContext.UserPermissions.Contains(Permissions.Polls.DeleteAny);

        if (!canDeleteAny)
        {
            var accessResult = cityAccessPolicy.Check(poll.CityId);
            if (!accessResult.IsSuccess)
                return accessResult.Errors[0];
            
            var guardResult = PollGuard.For(poll)
                .IsNotFinished()
                .EditWindowNotExpired()
                .Validate();

            if (!guardResult.IsSuccess)
                return guardResult.Errors[0];
        }

        unitOfWork.Polls.Delete(poll);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
