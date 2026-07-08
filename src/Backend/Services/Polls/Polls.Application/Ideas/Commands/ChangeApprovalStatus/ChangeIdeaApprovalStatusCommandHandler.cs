using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;
using Polls.Domain.Ideas;

namespace Polls.Application.Ideas.Commands.ChangeApprovalStatus;

public sealed class ChangeIdeaApprovalStatusCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeIdeaApprovalStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        ChangeIdeaApprovalStatusCommand command,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetByIdAsync(command.Id, cancellationToken);

        if (idea is null)
            return IdeaErrors.NotFound(command.Id);
        
        if (idea.ApprovalStatus == command.NewApprovalStatus)
            return Result<Unit>.Success(Unit.Value);

        idea.ApprovalStatus = command.NewApprovalStatus;
        unitOfWork.Ideas.Update(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}
