using System.Security.Claims;
using System.Text.Encodings.Web;
using CityPulse.Contracts.Auth.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Users.Tests.Infrastructure.Constants;

namespace Users.Tests.Infrastructure;

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = AuthConstants.SchemeName;

    private static readonly string[] DefaultPermissions =
    [
        AuthConstants.Permissions.ReadMe,
        AuthConstants.Permissions.ReadAny,
        AuthConstants.Permissions.UpdateMe,
        AuthConstants.Permissions.UpdateAny,
        AuthConstants.Permissions.DeleteAny,
        AuthConstants.Permissions.ActivateAny,
        AuthConstants.Permissions.DeactivateAny
    ];

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey(AuthConstants.Headers.Anonymous))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) };

        claims.AddRange(GetPermissions().Select(p => new Claim(AuthConstants.PermissionsClaimType, p)));

        if (Request.Headers.TryGetValue(AuthConstants.Headers.InternalUserId, out var internalUserId))
        {
            claims.Add(new Claim(CityPulseClaims.InternalUserId, internalUserId.ToString()));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private IEnumerable<string> GetPermissions() =>
        Request.Headers.TryGetValue(AuthConstants.Headers.Permissions, out var permissions)
            ? permissions.ToString().Split(
                AuthConstants.Headers.PermissionsSeparator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(p => p != AuthConstants.Headers.NoPermissions)
            : DefaultPermissions;
}
