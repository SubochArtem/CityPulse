using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Gateway.API.Constants;
using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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

    private static Task SignInAsync(HttpContext context)
    {
        var identity = new ClaimsIdentity(
            [new Claim(BffDefaults.SubjectClaim, TestConstants.Bff.UserId)],
            BffDefaults.CookieScheme);
        var properties = new AuthenticationProperties();
        properties.StoreTokens(
        [
            new AuthenticationToken
            {
                Name = BffDefaults.AccessTokenName,
                Value = CreateAccessToken(
                    TestConstants.Bff.PollsReadPermission,
                    TestConstants.Bff.IdeasCreatePermission)
            }
        ]);

        return context.SignInAsync(BffDefaults.CookieScheme, new ClaimsPrincipal(identity), properties);
    }
}
