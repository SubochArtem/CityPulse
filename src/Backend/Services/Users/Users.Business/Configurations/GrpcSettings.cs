namespace Users.Business.Configurations;

public class GrpcSettings
{
    public const string SectionName = "GrpcSettings";
    public required string CitiesServiceUrl { get; init; }
}
