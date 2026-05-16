namespace Polls.Infrastructure.Persistence.Options;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public required string Endpoint { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public required string BucketName { get; set; }
    public bool UseSsl { get; set; } = false;
}
