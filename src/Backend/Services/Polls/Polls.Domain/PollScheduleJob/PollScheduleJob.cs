namespace Polls.Domain.PollScheduleJob;

public class PollScheduleJob
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public required string HangfireJobId { get; set; }
}
