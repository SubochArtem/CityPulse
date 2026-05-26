using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Users.Business.Interfaces;

namespace Users.Presentation.Authorization;

public sealed class UserClaimsTransformation(IUserService userService) : IClaimsTransformation
{
    private readonly IUserService _userService = userService;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identityId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identityId is null || Guid.TryParse(identityId, out _))
            return principal;

        var user = await _userService.GetUserByIdentityIdAsync(identityId);
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
