using Users.DataAccess.Entities;

namespace Users.DataAccess.Interfaces;

public interface IUserUnitOfWorkRepository: IRepository<User>, IUnitOfWork
{
    Task<User?> GetByAuth0UserIdAsync(string auth0UserId, CancellationToken cancellationToken = default);
}
