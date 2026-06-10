using System.Security.Claims;
using CityPulse.Contracts.Auth.Claims;

namespace Polls.API.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) 
            ? id 
            : Guid.Empty;
    }

    public static Guid GetCityId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(CityPulseClaims.CityId);
        
        return Guid.TryParse(claim, out var id) 
            ? id 
            : Guid.Empty;
    }
}
