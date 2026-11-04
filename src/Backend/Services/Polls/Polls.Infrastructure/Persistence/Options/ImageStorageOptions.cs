using System.ComponentModel.DataAnnotations;

namespace Polls.Infrastructure.Persistence.Options;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    [Required(AllowEmptyStrings = false)]
    public required string Endpoint { get; set; }

    [Required(AllowEmptyStrings = false)]
    public required string AccessKey { get; set; }

    [Required(AllowEmptyStrings = false)]
    public required string SecretKey { get; set; }

    [Required(AllowEmptyStrings = false)]
    public required string BucketName { get; set; }

    [Required(AllowEmptyStrings = false)]
    [Url]
    public required string PublicBaseUrl { get; set; }

    public bool UseSsl { get; set; } = false;
}
