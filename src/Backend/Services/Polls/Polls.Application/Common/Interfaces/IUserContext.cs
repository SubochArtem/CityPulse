namespace Polls.Application.Common.Interfaces;

public interface IUserContextService
{
    Guid UserId { get; }

    IReadOnlySet<string> UserPermissions { get; }
}
