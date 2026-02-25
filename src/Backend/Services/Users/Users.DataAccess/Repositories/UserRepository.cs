using Microsoft.EntityFrameworkCore;
using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;

namespace Users.DataAccess.Repositories;

public class UserRepository(ApplicationDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByIdentityIdAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
    }
}
