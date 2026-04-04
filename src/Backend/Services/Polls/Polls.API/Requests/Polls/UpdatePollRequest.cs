namespace Polls.API.Requests.Polls;

public record UpdatePollRequest(
    string Title,
    string? Description,
    DateTimeOffset EndsAt,
    decimal BudgetAmount);
