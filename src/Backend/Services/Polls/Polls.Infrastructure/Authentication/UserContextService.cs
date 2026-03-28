using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Authorization;

namespace Polls.Infrastructure.Authentication;

public sealed class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    private IReadOnlySet<string>? _userPermissions;

    public Guid UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : Guid.Empty;
        }
    }

    public IReadOnlySet<string> UserPermissions
    {
        get
        {
            if (_userPermissions is not null)
                return _userPermissions;

            var user = httpContextAccessor.HttpContext?.User;

            var permissionsFromToken = user?.FindAll(Permissions.ClaimType).Select(c => c.Value)
                                       ?? Array.Empty<string>();

            _userPermissions = new HashSet<string>(permissionsFromToken);

            return _userPermissions;
        }
    }
}
