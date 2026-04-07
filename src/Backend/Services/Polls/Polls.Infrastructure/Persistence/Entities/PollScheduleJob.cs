namespace Polls.Infrastructure.Persistence.Entities;

public class PollScheduleJob
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public required string HangfireJobId { get; set; }
}
