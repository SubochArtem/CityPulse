using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;

namespace Polls.Infrastructure.Persistence.Extensions;

public class IdeaQueryBuilder(IQueryable<Idea> query)
{
    private IQueryable<Idea> _query = query;

    public IdeaQueryBuilder WithPollId(Guid? pollId)
    {
        if (pollId is not null)
            _query = _query.Where(i => i.PollId == pollId);
        return this;
    }

    public IdeaQueryBuilder WithStatus(IdeaStatus? status)
    {
        if (status is not null)
            _query = _query.Where(i => i.Status == status);
        return this;
    }

    public IdeaQueryBuilder WithSearchTerm(string? searchTerm)
    {
        if (searchTerm is null)
            return this;

        var lower = searchTerm.ToLower();
        _query = _query.Where(i =>
            i.Title.ToLower().Contains(lower)
            || (i.Description != null
                && i.Description.ToLower().Contains(lower)));
        return this;
    }

    public IQueryable<Idea> Build()
    {
        return _query;
    }
}
