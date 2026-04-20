using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.Guards;
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
            var validationResult = idea.ValidateIdeaAccess(command.UserId);
            if (!validationResult.IsSuccess)
                return validationResult.Error;
        }

        unitOfWork.Ideas.Delete(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
