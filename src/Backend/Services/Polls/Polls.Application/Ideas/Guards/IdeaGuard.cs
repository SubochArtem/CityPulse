using MediatR;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Ideas.Guards;

public sealed class IdeaGuard(Idea idea)
{
    private Error? _error;

    public static IdeaGuard For(Idea idea)
    {
        return new IdeaGuard(idea);
    }
    
    public IdeaGuard IsOwner(Guid userId)
    {
        if (_error is null && idea.UserId != userId)
            _error = IdeaErrors.NotOwner(idea.Id);
        return this;
    }
    
    public IdeaGuard IsNotApproved()
    {
        if (_error is null && idea.Status == IdeaStatus.Approved)
            _error = IdeaErrors.AlreadyApproved(idea.Id);
        return this;
    }

    public Result<Unit> Validate()
    {
        if (_error is not null) 
            return _error;

        return Result<Unit>.Success(Unit.Value);
    }
}
