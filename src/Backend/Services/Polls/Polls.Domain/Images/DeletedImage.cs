namespace Polls.Domain.Images;

public class DeletedImage
{
    public Guid Id { get; set; }
    public required string FileName { get; set; }
    public DateTimeOffset QueuedAt { get; set; } = DateTimeOffset.UtcNow;
}
