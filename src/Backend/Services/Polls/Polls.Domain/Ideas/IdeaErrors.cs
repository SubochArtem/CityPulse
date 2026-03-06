using Polls.Domain.Common;

namespace Polls.Domain.Ideas;

public static class IdeaErrors
{
    public static Error NotFound => Error.NotFound("Idea not found");
}
