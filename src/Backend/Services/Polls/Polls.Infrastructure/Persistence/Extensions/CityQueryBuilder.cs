using Microsoft.EntityFrameworkCore;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;

namespace Polls.Infrastructure.Persistence.Extensions;

public class CityQueryBuilder(IQueryable<City> query)
{
    private IQueryable<City> _query = query;

    public CityQueryBuilder WithStatus(CityStatus? status)
    {
        if (status is not null)
            _query = _query.Where(c => c.Status == status);
        return this;
    }

    public CityQueryBuilder WithSearchTerm(string? searchTerm)
    {
        if (searchTerm is null)
            return this;

        var lower = searchTerm.ToLower();
        _query = _query.Where(c =>
            c.Title.ToLower().Contains(lower)
            || (c.Description != null
                && c.Description.ToLower().Contains(lower)));
        return this;
    }
    
    public CityQueryBuilder IncludeImages(bool include = true)
    {
        if (include)
        {
            _query = _query.Include(c => c.Images);
        }
        return this;
    }

    public IQueryable<City> Build()
    {
        return _query;
    }
}
