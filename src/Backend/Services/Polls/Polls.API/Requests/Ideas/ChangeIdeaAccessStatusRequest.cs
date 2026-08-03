using Polls.Domain.Ideas.Enums;

namespace Polls.API.Requests.Ideas;

public record ChangeIdeaAccessStatusRequest
{
    public required IdeaAccessStatus NewIdeaAccessStatus { get; init; }
}
