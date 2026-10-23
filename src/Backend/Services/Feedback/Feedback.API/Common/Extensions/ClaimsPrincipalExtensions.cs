using System.Security.Claims;
using CityPulse.Contracts.Auth.Claims;

namespace Feedback.API.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetCityId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(CityPulseClaims.CityId);

        return Guid.TryParse(claim, out var id)
            ? id
            : Guid.Empty;
    }
}
