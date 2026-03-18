namespace Polls.Application.Polls.DTOs;

public record PollDto(
    Guid Id,
    Guid CityId,
    DateTime StartedAt,
    DateTime EndsAt,
    int Type,
    decimal BudgetAmount,
    int Status);
