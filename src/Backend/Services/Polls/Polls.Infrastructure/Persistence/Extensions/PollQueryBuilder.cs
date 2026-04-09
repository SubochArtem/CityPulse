using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Infrastructure.Persistence.Extensions;

public class PollQueryBuilder(IQueryable<Poll> query)
{
    private IQueryable<Poll> _query = query;

    public PollQueryBuilder WithCityId(Guid? cityId)
    {
        if (cityId is not null)
            _query = _query.Where(p => p.CityId == cityId);
        return this;
    }

    public PollQueryBuilder WithType(PollType? type)
    {
        if (type is not null)
            _query = _query.Where(p => p.Type == type);
        return this;
    }

    public PollQueryBuilder WithStatus(PollStatus? status)
    {
        if (status is not null)
            _query = _query.Where(p => p.Status == status);
        return this;
    }
    
    public PollQueryBuilder WithSearchTerm(string? searchTerm)
    {
        if (searchTerm is null)
            return this;

        var lower = searchTerm.ToLower();
        _query = _query.Where(p =>
            p.Title.ToLower().Contains(lower)
            || (p.Description != null
                && p.Description.ToLower().Contains(lower)));
        return this;
    }

    public IQueryable<Poll> Build()
    {
        return _query;
    }
}
