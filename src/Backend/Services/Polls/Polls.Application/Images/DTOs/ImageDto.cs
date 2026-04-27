namespace Polls.Application.Images.DTOs;

public class ImageDto
{
    public Guid Id { get; init; }
    public required string Url { get; init; }
    public int Order { get; init; }
}
