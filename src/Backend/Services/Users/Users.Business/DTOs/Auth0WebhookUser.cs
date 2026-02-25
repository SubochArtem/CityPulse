using System.Text.Json.Serialization;

namespace Users.Business.DTOs;

public class Auth0WebhookUser
{
    [JsonPropertyName("id")] 
    public string? Id { get; init; }
}
