namespace Polls.Domain.Images;

public class Image
{
    public Guid Id { get; set; }
    public required string FileName { get; set; }
    public int Order { get; set; }
}
