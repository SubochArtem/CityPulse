using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;
using Polls.Domain.Ideas;

namespace Polls.Application.Ideas.Commands.ChangeAccessStatus;

public sealed class ChangeIdeaAccessStatusCommandHandler(
    IUnitOfWork unitOfWork) 
    : IRequestHandler<ChangeIdeaAccessStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        ChangeIdeaAccessStatusCommand command, 
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetByIdAsync(command.Id, cancellationToken);
        if (idea is null)
            return IdeaErrors.NotFound(command.Id);
        
        if (idea.AccessStatus == command.NewIdeaAccessStatus)
            return Result<Unit>.Success(Unit.Value);
        
        idea.AccessStatus = command.NewIdeaAccessStatus;
        unitOfWork.Ideas.Update(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}
