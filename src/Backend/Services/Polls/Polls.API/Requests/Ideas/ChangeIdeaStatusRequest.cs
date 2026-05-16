using Polls.Domain.Ideas.Enums;

namespace Polls.API.Requests.Ideas;

public record ChangeIdeaStatusRequest
{
    public required IdeaStatus NewStatus { get; init; }
}
