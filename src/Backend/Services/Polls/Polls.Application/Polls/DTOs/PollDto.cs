namespace Polls.Application.Polls.DTOs;

public record PollDto(
    Guid Id,
    Guid CityId,
    DateTime StartedAt,
    DateTime EndsAt,
    string Type,
    decimal BudgetAmount,
    string Status);
