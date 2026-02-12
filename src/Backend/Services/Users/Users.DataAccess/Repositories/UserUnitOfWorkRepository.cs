using Microsoft.EntityFrameworkCore;
using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;

namespace Users.DataAccess.Repositories;

public class UserUnitOfWorkRepository(ApplicationDbContext context)
    : Repository<User>(context), IUserUnitOfWorkRepository
{
    public async Task<User?> GetByAuth0UserIdAsync(
        string auth0UserId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId, cancellationToken);
    }
    
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
