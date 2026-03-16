using Polls.Application.Ideas.DTOs;

namespace Polls.Application.Polls.DTOs;

public record PollWithIdeasDto(
    Guid Id,
    Guid CityId,
    DateTime StartedAt,
    DateTime EndsAt,
    string Type,
    decimal BudgetAmount,
    string Status,
    IReadOnlyCollection<IdeaDto> Ideas);
