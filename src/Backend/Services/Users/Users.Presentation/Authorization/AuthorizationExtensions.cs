using Microsoft.AspNetCore.Authorization;
using Users.Presentation.Authorization.Handlers;
using Users.Presentation.Authorization.Requirements;

namespace Users.Presentation.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, SelfOrAnyHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.ReadUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.Requirements.Add(
                    new SelfOrAnyRequirement(
                        Permissions.UsersReadMe,
                        Permissions.UsersReadAny));
            });

            options.AddPolicy(Policies.UpdateUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.Requirements.Add(
                    new SelfOrAnyRequirement(
                        Permissions.UsersUpdateMe,
                        Permissions.UsersUpdateAny));
            });

            options.AddPolicy(Policies.DeleteUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(Permissions.ClaimType, Permissions.UsersDeleteAny);
            });

            options.AddPolicy(Policies.ActivateUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(Permissions.ClaimType, Permissions.UsersActivateAny);
            });

            options.AddPolicy(Policies.DeactivateUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(Permissions.ClaimType, Permissions.UsersDeactivateAny);
            });
        });

        return services;
    }
}
