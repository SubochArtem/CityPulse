using System.Security.Claims;
using System.Text.Encodings.Web;
using CityPulse.Contracts.Auth.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polls.Tests.Infrastructure.Constants;

namespace Polls.Tests.Infrastructure;

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = AuthConstants.SchemeName;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey(AuthConstants.Headers.Anonymous))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) };
        claims.AddRange(GetPermissions().Select(permission => new Claim(AuthConstants.PermissionsClaimType, permission)));

        if (Request.Headers.TryGetValue(AuthConstants.Headers.UserId, out var userId))
            claims.Add(new Claim(CityPulseClaims.InternalUserId, userId.ToString()));

        if (Request.Headers.TryGetValue(AuthConstants.Headers.CityId, out var cityId))
            claims.Add(new Claim(CityPulseClaims.CityId, cityId.ToString()));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string[] GetPermissions() =>
        Request.Headers.TryGetValue(AuthConstants.Headers.Permissions, out var permissions)
            ? permissions
                .ToString()
                .Split(
                    AuthConstants.Headers.PermissionsSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];
}
