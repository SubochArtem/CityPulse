using Polls.Domain.Ideas.Enums;

namespace Polls.API.Requests.Ideas;

public record ChangeIdeaAccessStatusRequest
{
    public required AccessStatus NewAccessStatus { get; init; }
}
