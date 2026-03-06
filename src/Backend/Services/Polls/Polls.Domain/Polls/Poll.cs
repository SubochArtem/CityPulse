using Polls.Domain.Polls.Enums;

namespace Polls.Domain.Polls;

public class Poll
{
    public Guid Id { get; set; }
    public Guid CityId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndsAt { get; set; }
    public PollType Type { get; set; }
    public decimal BudgetAmount { get; set; }
    public PollStatus Status { get; set; } =  PollStatus.Active;
}
