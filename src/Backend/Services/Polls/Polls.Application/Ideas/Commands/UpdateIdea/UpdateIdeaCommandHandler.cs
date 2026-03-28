using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Security;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Ideas.Guards;
using Polls.Application.Polls.Guards;
using Polls.Domain.Authorization;
using Polls.Domain.Common;
using Polls.Domain.Ideas;

namespace Polls.Application.Ideas.Commands.UpdateIdea;

public sealed class UpdateIdeaCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserContextService userContext,
    CityAccessPolicy cityAccessPolicy)
    : IRequestHandler<UpdateIdeaCommand, Result<IdeaDto>>
{
    public async Task<Result<IdeaDto>> Handle(
        UpdateIdeaCommand command,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetWithPollAsync(command.Id, cancellationToken);
        if (idea is null)
            return IdeaErrors.NotFound(command.Id);

        var canUpdateAny = userContext.UserPermissions.Contains(Permissions.Ideas.UpdateAny);

        if (!canUpdateAny)
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

        idea.Title = command.Title;
        idea.Description = command.Description;

        unitOfWork.Ideas.Update(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<IdeaDto>(idea);
    }
}
