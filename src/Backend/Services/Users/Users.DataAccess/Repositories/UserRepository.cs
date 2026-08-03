using CityPulse.Contracts.Querying.Pagination;
using Microsoft.EntityFrameworkCore;
using Users.DataAccess.Entities;
using Users.DataAccess.Extensions;
using Users.DataAccess.Interfaces;
using Users.DataAccess.Models;

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
    
    public async Task<PagedList<User>> GetFilteredAsync(
        UserFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await new UserQueryBuilder(_dbSet.AsNoTracking())
            .WithNickname(filter.Nickname)
            .WithCityId(filter.CityId)
            .Build()
            .ToPagedListAsync(filter.Page, filter.PageSize, cancellationToken);
    }
}
