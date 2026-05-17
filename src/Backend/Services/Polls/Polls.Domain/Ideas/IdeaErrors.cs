using Polls.Domain.Common;

namespace Polls.Domain.Ideas;

public static class IdeaErrors
{
    public static Error AlreadyApproved(Guid id) =>
        Error.Conflict($"Idea with id '{id}' was already approved and cannot be modified");

    public static Error NotFound(Guid id) =>
         Error.NotFound($"Idea with id '{id}' was not found");
    
    public static Error NotOwner(Guid id) =>
        Error.Forbidden($"Cant modify idea '{id}' because you are not the owner");
}
