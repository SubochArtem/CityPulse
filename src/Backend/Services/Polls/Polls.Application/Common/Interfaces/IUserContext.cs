namespace Polls.Application.Common.Interfaces;

public interface IUserContextService
{
    Guid UserId { get; }
    
    Guid CityId { get; }

    IReadOnlySet<string> UserPermissions { get; }
}
