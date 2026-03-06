using Polls.Domain.Common;
using Polls.Domain.Polls.Enums;

namespace Polls.Domain.Polls;

public class Poll : EntityBase
{
    public Guid CityId { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public PollType Type { get; set; }
    public decimal BudgetAmount { get; set; }
    public PollStatus Status { get; set; } = PollStatus.Active;
}
