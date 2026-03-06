using Polls.Domain.Common;

namespace Polls.Domain.Ideas;

public static class IdeaErrors
{
    public static Error NotFound => Error.NotFound("Idea not found");
    public static Error AlreadyInPoll => Error.Conflict("Idea is already in poll");
    public static Error NotPending => Error.Conflict("Idea is not in pending status");
}
