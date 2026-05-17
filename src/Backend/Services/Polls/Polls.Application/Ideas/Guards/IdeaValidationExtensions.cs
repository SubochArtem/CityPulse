using MediatR;
using Polls.Application.Polls.Guards;
using Polls.Domain.Common;
using Polls.Domain.Ideas;

namespace Polls.Application.Ideas.Guards;

public static class IdeaValidationExtensions
{
    public static Result<Unit> ValidateIdeaAccess(this Idea idea, Guid userId)
    {
        var ideaGuard = IdeaGuard.For(idea)
            .IsOwner(userId)
            .IsNotApproved()
            .Validate();
        if (!ideaGuard.IsSuccess)
            return ideaGuard.Error;

        var pollGuard = PollGuard.For(idea.Poll)
            .IsNotFinished()
            .Validate();
        if (!pollGuard.IsSuccess)
            return pollGuard.Error;

        return Unit.Value;
    }
}
