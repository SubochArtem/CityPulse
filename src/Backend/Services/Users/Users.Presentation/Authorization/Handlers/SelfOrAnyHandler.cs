using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Users.Business.Interfaces;
using Users.Presentation.Authorization.Requirements;

namespace Users.Presentation.Authorization.Handlers;

public sealed class SelfOrAnyHandler(
    IUserService userService,
    IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<SelfOrAnyRequirement>
{
    private const string RouteIdParameter = "id";
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IUserService _userService = userService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SelfOrAnyRequirement requirement)
    {
        if (context.User.HasClaim(Permissions.ClaimType, requirement.AnyPermission))
        {
            context.Succeed(requirement);
            return;
        }

        if (!context.User.HasClaim(Permissions.ClaimType, requirement.SelfPermission))
            return;

        var routeId = _httpContextAccessor.HttpContext?.GetRouteValue(RouteIdParameter)?.ToString();
        var identityId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (routeId is null || identityId is null)
            return;

        if (!Guid.TryParse(routeId, out var routeGuid))
            return;

        var currentUser = await _userService.GetUserByAuth0IdAsync(identityId);

        if (currentUser is not null && currentUser.Id == routeGuid)
            context.Succeed(requirement);
    }
}
