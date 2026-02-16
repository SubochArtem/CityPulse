namespace Users.Business.Configurations;

public class Auth0Settings
{
    public string Domain { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;

    public string Audience { get; set; } = null!;

    public string ManagementApiClientId { get; set; } = null!;

    public string ManagementApiClientSecret { get; set; } = null!;

    public string ManagementApiAudience { get; set; } = null!;
}
