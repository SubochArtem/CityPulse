using System.Security.Claims;

namespace Polls.API.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    private const string CityIdClaim = "https://citypulse.com/city_id";

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) 
            ? id 
            : Guid.Empty;
    }

    public static Guid GetCityId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(CityIdClaim);
        return Guid.TryParse(claim, out var id) 
            ? id 
            : Guid.Empty;
    }
}
