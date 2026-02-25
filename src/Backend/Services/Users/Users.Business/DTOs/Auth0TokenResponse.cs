using System.Text.Json.Serialization;

namespace Users.Business.DTOs;

public class Auth0TokenResponse
{
    [JsonPropertyName("access_token")] 
    public required string AccessToken { get; set; }

    [JsonPropertyName("expires_in")] 
    public int ExpiresIn { get; set; }
}
