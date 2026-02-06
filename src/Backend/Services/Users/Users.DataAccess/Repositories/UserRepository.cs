using Microsoft.EntityFrameworkCore;
using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;

namespace Users.DataAccess.Repositories;

public class UserRepository(ApplicationDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByAuth0UserIdAsync(string auth0UserId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId);
    }
}
