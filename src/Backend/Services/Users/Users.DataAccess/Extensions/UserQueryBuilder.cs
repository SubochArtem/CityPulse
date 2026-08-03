using Users.DataAccess.Entities;

namespace Users.DataAccess.Extensions;

public class UserQueryBuilder(IQueryable<User> query)
{
    private IQueryable<User> _query = query;

    public UserQueryBuilder WithNickname(string? nickname)
    {
        if (!string.IsNullOrWhiteSpace(nickname))
            _query = _query.Where(u => u.Nickname.ToLower().Contains(nickname.ToLower()));
        return this;
    }

    public UserQueryBuilder WithCityId(Guid? cityId)
    {
        if (cityId is not null)
            _query = _query.Where(u => u.CityId == cityId);
        return this;
    }

    public IQueryable<User> Build() => _query;
}
