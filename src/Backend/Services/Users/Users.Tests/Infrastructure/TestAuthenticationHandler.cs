using System.Security.Claims;
using System.Text.Encodings.Web;
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

    private static readonly Claim[] DefaultClaims =
    [
        new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        new(AuthConstants.PermissionsClaimType, AuthConstants.Permissions.ReadMe),
        new(AuthConstants.PermissionsClaimType, AuthConstants.Permissions.ReadAny),
        new(AuthConstants.PermissionsClaimType, AuthConstants.Permissions.UpdateMe),
        new(AuthConstants.PermissionsClaimType, AuthConstants.Permissions.UpdateAny),
        new(AuthConstants.PermissionsClaimType, AuthConstants.Permissions.DeleteAny),
        new(AuthConstants.PermissionsClaimType, AuthConstants.Permissions.ActivateAny),
        new(AuthConstants.PermissionsClaimType, AuthConstants.Permissions.DeactivateAny)
    ];

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(DefaultClaims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
