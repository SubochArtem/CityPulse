using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;
using Polls.Domain.Ideas;

namespace Polls.Application.Ideas.Commands.ChangeStatus;

public sealed class ChangeIdeaStatusCommandHandler(
    IUnitOfWork unitOfWork) 
    : IRequestHandler<ChangeIdeaStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        ChangeIdeaStatusCommand command, 
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetByIdAsync(command.Id, cancellationToken);
        if (idea is null)
            return IdeaErrors.NotFound(command.Id);
        
        if (idea.Status == command.NewStatus)
            return Result<Unit>.Success(Unit.Value);
        
        idea.Status = command.NewStatus;
        unitOfWork.Ideas.Update(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}
