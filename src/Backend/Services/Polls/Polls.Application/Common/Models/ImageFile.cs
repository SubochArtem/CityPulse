namespace Polls.Application.Common.Models;

public class ImageFile
{
    public required Stream Content { get; set; }
    public required string FileName { get; set; }
    public required string ContentType  { get; set; }
}
