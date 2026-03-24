namespace Polls.Application.Polls.DTOs;

public record PollDto(
    Guid Id,
    string Title,
    string? Description,
    Guid CityId,
    DateTime StartedAt,
    DateTime EndsAt,
    int Type,
    decimal BudgetAmount,
    int Status);
