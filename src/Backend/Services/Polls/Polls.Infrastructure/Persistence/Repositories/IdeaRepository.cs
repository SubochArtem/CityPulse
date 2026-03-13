using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Domain.Ideas;
using Polls.Infrastructure.Persistence.Extensions;

namespace Polls.Infrastructure.Persistence.Repositories;

public class IdeaRepository(ApplicationDbContext context)
    : Repository<Idea>(context), IIdeaRepository
{
    public async Task<PagedList<Idea>> GetFilteredAsync(
        IdeaFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await new IdeaQueryBuilder(_dbSet.AsNoTracking())
            .WithPollId(filter.PollId)
            .WithStatus(filter.Status)
            .WithSearchTerm(filter.SearchTerm)
            .Build()
            .ToPagedListAsync(filter.Page, filter.PageSize, cancellationToken);
    }
}
