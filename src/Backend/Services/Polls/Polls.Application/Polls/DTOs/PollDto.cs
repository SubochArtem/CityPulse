namespace Polls.Application.Polls.DTOs;

public class PollDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public Guid CityId { get; init; }
    public DateTimeOffset EndsAt { get; init; }
    public int Type { get; init; }
    public decimal BudgetAmount { get; init; }
    public int Status { get; init; }
}
