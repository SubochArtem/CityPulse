namespace Feedback.API.Infrastructure.Grpc;

public class GrpcSettings
{
    public const string SectionName = "GrpcSettings";
    public required string IdeasServiceUrl { get; init; }
}
