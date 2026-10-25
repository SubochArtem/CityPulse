using CityPulse.Contracts.Querying.Pagination;
using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Models;
using Polls.Domain.Common;

namespace Polls.Infrastructure.Persistence.Extensions;

public static class PagedListExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        where T : EntityBase
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, page, pageSize, totalCount);
    }
}
