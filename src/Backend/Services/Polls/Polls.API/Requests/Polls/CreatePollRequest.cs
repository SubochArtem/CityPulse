using Polls.Domain.Polls.Enums;

namespace Polls.API.Requests.Polls;

public record CreatePollRequest(
    string Title,
    string? Description,
    PollType Type,
    DateTimeOffset EndsAt,
    decimal BudgetAmount);
