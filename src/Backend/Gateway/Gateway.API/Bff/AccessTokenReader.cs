using System.IdentityModel.Tokens.Jwt;
using Gateway.API.Constants;

namespace Gateway.API.Bff;

public static class AccessTokenReader
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    public static IReadOnlyList<string> ReadPermissions(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken) || !Handler.CanReadToken(accessToken))
        {
            return [];
        }

        return Handler.ReadJwtToken(accessToken).Claims
            .Where(claim => claim.Type == BffDefaults.PermissionsClaim)
            .Select(claim => claim.Value)
            .Distinct()
            .ToArray();
    }
}
