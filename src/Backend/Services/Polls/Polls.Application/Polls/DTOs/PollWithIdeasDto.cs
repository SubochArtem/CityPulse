using Polls.Application.Ideas.DTOs;

namespace Polls.Application.Polls.DTOs;

public record PollWithIdeasDto(
    Guid Id,
    Guid CityId,
    DateTime StartedAt,
    DateTime EndsAt,
    int Type,
    decimal BudgetAmount,
    int Status,
    IReadOnlyCollection<IdeaDto> Ideas);
