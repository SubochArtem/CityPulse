using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Ideas.Guards;
using Polls.Application.Polls.Guards;
using Polls.Domain.Common;
using Polls.Domain.Ideas;

namespace Polls.Application.Ideas.Commands.UpdateIdea;

public sealed class UpdateIdeaCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateIdeaCommand, Result<IdeaDto>>
{
    public async Task<Result<IdeaDto>> Handle(
        UpdateIdeaCommand command,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetWithPollAsync(command.Id, cancellationToken);
        if (idea is null)
            return IdeaErrors.NotFound(command.Id);

        if (!command.BypassRestrictions)
        {
            var ideaGuard = IdeaGuard.For(idea)
                .IsOwner(command.UserId)
                .IsNotApproved()
                .Validate();
            if (!ideaGuard.IsSuccess)
                return ideaGuard.Error;

            var guardResult = PollGuard.For(idea.Poll)
                .IsNotFinished()
                .Validate();
            if (!guardResult.IsSuccess)
                return guardResult.Error;
        }

        idea.Title = command.Title;
        idea.Description = command.Description;

        unitOfWork.Ideas.Update(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<IdeaDto>(idea);
    }
}
