using System.Text.Json.Serialization;

namespace Users.Business.DTOs;

public class Auth0WebhookPayload
{
    [JsonPropertyName("event")] 
    public string? Event { get; init; }

    [JsonPropertyName("iss")] 
    public string? Iss { get; init; }

    [JsonPropertyName("user")] 
    public Auth0WebhookUser? User { get; init; }
}
