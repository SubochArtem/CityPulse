namespace Polls.API.Requests.Polls;

public record UpdatePollRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required DateTimeOffset EndsAt { get; init; }
    public required decimal BudgetAmount { get; init; }
}
