using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Gateway.API.Constants;
using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Gateway.Tests.Infrastructure;

public sealed class BffTestSignInStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Map(TestConstants.Bff.TestSignInPath, branch => branch.Run(SignInAsync));
        next(app);
    };

    public static string CreateAccessToken(params string[] permissions)
    {
        var claims = permissions.Select(permission => new Claim(BffDefaults.PermissionsClaim, permission));

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: claims));
    }

    public static string SignInPath(int expiresInSeconds, string? refreshToken = null)
    {
        var query = new Dictionary<string, string?>
        {
            [TestConstants.Bff.ExpiresInParam] = expiresInSeconds.ToString(CultureInfo.InvariantCulture)
        };
        if (refreshToken is not null)
        {
            query[TestConstants.Bff.RefreshTokenParam] = refreshToken;
        }

        return QueryHelpers.AddQueryString(TestConstants.Bff.TestSignInPath, query);
    }

    private static Task SignInAsync(HttpContext context)
    {
        var identity = new ClaimsIdentity(
            [new Claim(BffDefaults.SubjectClaim, TestConstants.Bff.UserId)],
            BffDefaults.CookieScheme);
        var properties = new AuthenticationProperties();
        properties.StoreTokens(CreateTokens(context.Request.Query));

        return context.SignInAsync(BffDefaults.CookieScheme, new ClaimsPrincipal(identity), properties);
    }

    private static List<AuthenticationToken> CreateTokens(IQueryCollection query)
    {
        var tokens = new List<AuthenticationToken>
        {
            new()
            {
                Name = BffDefaults.AccessTokenName,
                Value = CreateAccessToken(
                    TestConstants.Bff.PollsReadPermission,
                    TestConstants.Bff.IdeasCreatePermission)
            }
        };

        if (int.TryParse(query[TestConstants.Bff.ExpiresInParam], CultureInfo.InvariantCulture, out var expiresIn))
        {
            tokens.Add(new AuthenticationToken
            {
                Name = BffDefaults.ExpiresAtName,
                Value = DateTimeOffset.UtcNow.AddSeconds(expiresIn).ToString("o", CultureInfo.InvariantCulture)
            });
        }

        var refreshToken = query[TestConstants.Bff.RefreshTokenParam].ToString();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            tokens.Add(new AuthenticationToken { Name = BffDefaults.RefreshTokenName, Value = refreshToken });
        }

        return tokens;
    }
}
