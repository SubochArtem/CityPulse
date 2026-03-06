using Polls.Domain.Common;

namespace Polls.Domain.Ideas;

public static class IdeaErrors
{
    public static Error NotFound => Error.NotFound("Idea not found");
    public static Error PollAlreadyFinished => Error.Conflict("Cannot create idea in finished poll");
    public static Error AlreadyApproved => Error.Conflict("Cannot modify approved idea");
}
