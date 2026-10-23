using System.ComponentModel.DataAnnotations;

namespace Feedback.API.Infrastructure.Grpc;

public class GrpcSettings
{
    public const string SectionName = "GrpcSettings";

    [Required]
    public required string IdeasServiceUrl { get; init; }
}
