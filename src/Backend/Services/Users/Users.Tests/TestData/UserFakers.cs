using Bogus;
using Users.Business.DTOs;
using Users.DataAccess.Entities;
using Users.DataAccess.Entities.Enums;

namespace Users.Tests.TestData;

internal static class UserFakers
{
    private const int Seed = 20260721;
    private const string DefaultLocale = "en";
    private const string NicknameAllowedChars = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int DefaultNicknameLength = 12;

    public static Faker<User> Users() =>
        new Faker<User>(DefaultLocale)
            .UseSeed(Seed)
            .StrictMode(true)
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(x => x.UpdatedAt, f => f.Date.RecentOffset())
            .RuleFor(x => x.IdentityId, f => $"auth0|{f.Random.AlphaNumeric(16)}")
            .RuleFor(x => x.Nickname, ValidNickname)
            .RuleFor(x => x.CityId, f => f.Random.Guid())
            .RuleFor(x => x.AccessStatus, _ => UserAccessStatus.Active);

    public static Faker<CreateUserDto> CreateUserDtos() =>
        new Faker<CreateUserDto>(DefaultLocale)
            .UseSeed(Seed)
            .StrictMode(true)
            .RuleFor(x => x.IdentityId, f => $"auth0|{f.Random.AlphaNumeric(16)}")
            .RuleFor(x => x.Nickname, ValidNickname);

    public static Faker<UpdateUserProfileDto> UpdateUserProfileDtos() =>
        new Faker<UpdateUserProfileDto>(DefaultLocale)
            .UseSeed(Seed)
            .StrictMode(true)
            .RuleFor(x => x.Nickname, ValidNickname)
            .RuleFor(x => x.CityId, f => f.Random.Guid());

    public static Faker<UserFilterDto> UserFilterDtos() =>
        new Faker<UserFilterDto>(DefaultLocale)
            .UseSeed(Seed)
            .StrictMode(true)
            .RuleFor(x => x.Page, _ => 1)
            .RuleFor(x => x.PageSize, _ => 10)
            .RuleFor(x => x.Nickname, ValidNickname)
            .RuleFor(x => x.CityId, f => f.Random.Guid());

    private static string ValidNickname(Faker f) =>
        f.Random.String2(DefaultNicknameLength, NicknameAllowedChars);
}
