using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Users.Presentation.Authorization.Requirements;

namespace Users.Presentation.Authorization.Handlers;

public sealed class SelfOrAnyHandler(
    IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<SelfOrAnyRequirement>
{
    private const string RouteIdParameter = "id";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SelfOrAnyRequirement requirement)
    {
        if (context.User.HasClaim(Permissions.ClaimType, requirement.AnyPermission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var routeId = httpContextAccessor.HttpContext?.GetRouteValue(RouteIdParameter)?.ToString();
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!context.User.HasClaim(Permissions.ClaimType, requirement.SelfPermission)
            || routeId is null
            || userId is null
            || !Guid.TryParse(routeId, out var routeGuid)
            || !Guid.TryParse(userId, out var userGuid))
            return Task.CompletedTask;

        if (userGuid == routeGuid)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
