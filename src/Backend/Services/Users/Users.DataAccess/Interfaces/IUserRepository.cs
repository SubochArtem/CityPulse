using Users.DataAccess.Entities;
using Users.DataAccess.Models;

namespace Users.DataAccess.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdentityIdAsync(
        string identityId, 
        CancellationToken cancellationToken = default);
    
    Task<PagedList<User>> GetFilteredAsync(
        UserFilter filter,
        CancellationToken cancellationToken = default);
}
