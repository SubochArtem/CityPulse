using System.Text.Json.Serialization;

namespace Gateway.API.Bff;

public sealed record TokenRefreshResponse(
    [property: JsonPropertyName("access_token")] string? AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("id_token")] string? IdToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);
