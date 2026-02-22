using Microsoft.AspNetCore.Authorization;

namespace Users.Presentation.Authorization.Requirements;

public sealed class SelfOrAnyRequirement(
    string selfPermission, 
    string anyPermission) : IAuthorizationRequirement
{
    public string SelfPermission { get; } = selfPermission;
    public string AnyPermission { get; } = anyPermission;
}

