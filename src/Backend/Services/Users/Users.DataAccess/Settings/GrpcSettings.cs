namespace Users.DataAccess.Settings;

public class GrpcSettings
{
    public const string SectionName = "GrpcSettings";
    public required string CitiesServiceUrl { get; init; }
}
