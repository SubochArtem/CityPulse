using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.Guards;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Images;

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

        if (idea.Images.Count > 0)
        {
            var deletedImages = idea.Images
                .Select(i => new DeletedImage { FileName = i.FileName })
                .ToList();

            unitOfWork.DeletedImages.AddRange(deletedImages);
        }

        unitOfWork.Ideas.Delete(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
