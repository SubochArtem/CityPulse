using System.Text.Json.Serialization;

namespace Users.Business.DTOs;

public class Auth0UserResponse
{
    [JsonPropertyName("user_id")] 
    public required string UserId { get; set; }

    [JsonPropertyName("email")] 
    public string? Email { get; set; }

    [JsonPropertyName("email_verified")] 
    public bool EmailVerified { get; set; }

    [JsonPropertyName("name")] 
    public string? Name { get; set; }

    [JsonPropertyName("nickname")] 
    public string? Nickname { get; set; }

    [JsonPropertyName("picture")] 
    public string? Picture { get; set; }

    [JsonPropertyName("blocked")] 
    public bool Blocked { get; set; }

    [JsonPropertyName("created_at")] 
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")] 
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("last_login")] 
    public DateTimeOffset? LastLogin { get; set; }

    [JsonPropertyName("app_metadata")] 
    public Dictionary<string, object>? AppMetadata { get; set; }

    [JsonPropertyName("user_metadata")] 
    public Dictionary<string, object>? UserMetadata { get; set; }
}
