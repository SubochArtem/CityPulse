namespace Polls.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
}
