namespace Polls.API.Authorization;

public class Auth0Settings
{
    public required string Domain { get; set; }
    public required string Authority { get; set; }
    public required string Audience { get; set; }
    public required string ClientId { get; set; }
}
