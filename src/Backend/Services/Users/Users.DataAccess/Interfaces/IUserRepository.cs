using Users.DataAccess.Entities;

namespace Users.DataAccess.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByAuth0UserIdAsync(string auth0UserId);
}
