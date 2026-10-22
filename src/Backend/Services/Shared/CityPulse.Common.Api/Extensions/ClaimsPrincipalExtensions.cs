using System.Security.Claims;
using CityPulse.Contracts.Auth.Claims;

namespace CityPulse.Common.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(CityPulseClaims.InternalUserId);
        return Guid.TryParse(claim, out var id)
            ? id
            : Guid.Empty;
    }
}
