using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Common.Models;

public class IdeaFilter : BaseFilter
{
    public Guid? PollId { get; set; }
    public IdeaStatus? Status { get; set; }
    public string? SearchTerm { get; set; }
}
