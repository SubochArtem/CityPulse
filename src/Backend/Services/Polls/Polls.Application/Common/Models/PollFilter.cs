using Polls.Domain.Polls.Enums;

namespace Polls.Application.Common.Models;

public class PollFilter : BaseFilter
{
    public Guid? CityId { get; set; }
    public PollType? Type { get; set; }
    public PollStatus? Status { get; set; }
    public Guid? CorrelationId { get; set; }
}
