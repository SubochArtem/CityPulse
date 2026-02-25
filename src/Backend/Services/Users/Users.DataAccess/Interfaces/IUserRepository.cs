using Users.DataAccess.Entities;

namespace Users.DataAccess.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);
}
