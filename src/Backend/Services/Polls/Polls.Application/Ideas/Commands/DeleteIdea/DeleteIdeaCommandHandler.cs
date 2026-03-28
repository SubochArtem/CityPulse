using MediatR;
using Polls.Application.Common.Extensions;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Security;
using Polls.Application.Ideas.Guards;
using Polls.Application.Polls.Guards;
using Polls.Domain.Authorization;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Polls;

namespace Polls.Application.Ideas.Commands.DeleteIdea;

public sealed class DeleteIdeaCommandHandler(
    IUnitOfWork unitOfWork, 
    IUserContextService userContext,
    CityAccessPolicy cityAccessPolicy)
    : IRequestHandler<DeleteIdeaCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        DeleteIdeaCommand command,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetWithPollAsync(command.Id, cancellationToken);
        if (idea is null)
            return IdeaErrors.NotFound(command.Id);

        var canDeleteAny = userContext.UserPermissions.Contains(Permissions.Ideas.DeleteAny);

        if (!canDeleteAny)
        {
            var accessResult = cityAccessPolicy.Check(idea.Poll.CityId);
            if (!accessResult.IsSuccess)
                return accessResult.Errors[0];

            var ideaGuard = IdeaGuard.For(idea)
                .IsOwner(userContext.UserId)
                .IsNotApproved()
                .Validate();
            if (!ideaGuard.IsSuccess) 
                return ideaGuard.Errors[0];

            var guardResult = PollGuard.For(idea.Poll)
                .IsNotFinished()
                .Validate();
            if (!guardResult.IsSuccess)
                return guardResult.Errors[0];
        }

        unitOfWork.Ideas.Delete(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
