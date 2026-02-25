using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Users.Business.Interfaces;

namespace Users.Presentation.Authorization;

public sealed class UserClaimsTransformation(IUserService userService) : IClaimsTransformation
{
    private readonly IUserService _userService = userService;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var auth0UserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (auth0UserId is null || Guid.TryParse(auth0UserId, out _))
            return principal;

        var user = await _userService.GetUserByAuth0IdAsync(auth0UserId);
        if (user is null)
            return principal;

        var clone = principal.Clone();
        var identity = (ClaimsIdentity)clone.Identity!;

        var oldClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
        if (oldClaim is not null)
            identity.RemoveClaim(oldClaim);

        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

        return clone;
    }
}
