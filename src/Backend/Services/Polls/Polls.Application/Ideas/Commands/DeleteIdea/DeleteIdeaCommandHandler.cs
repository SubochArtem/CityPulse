using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.Guards;
using Polls.Application.Polls.Guards;
using Polls.Domain.Common;
using Polls.Domain.Ideas;

namespace Polls.Application.Ideas.Commands.DeleteIdea;

public sealed class DeleteIdeaCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteIdeaCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        DeleteIdeaCommand command,
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

        unitOfWork.Ideas.Delete(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
