using System.Text.Json.Serialization;

namespace Users.Business.DTOs;

public class Auth0TokenResponse
{
    [JsonPropertyName("access_token")] 
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("expires_in")] 
    public int ExpiresIn { get; set; }
}
