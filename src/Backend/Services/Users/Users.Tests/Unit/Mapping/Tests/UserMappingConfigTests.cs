using CityPulse.Contracts.Querying.Pagination;
using Mapster;
using Users.Business.DTOs;
using Users.Business.Mapping;
using Users.DataAccess.Entities;
using Users.DataAccess.Entities.Enums;
using Users.DataAccess.Models;
using Users.Tests.TestData;
using Users.Tests.Unit.Mapping.TestData;
using Xunit;

namespace Users.Tests.Unit.Mapping.Tests;

public sealed class UserMappingConfigTests
{
    private static TypeAdapterConfig BuildIsolatedConfig()
    {
        var config = new TypeAdapterConfig();
        UserMappingConfig.Configure(config);
        return config;
    }

    private static void AssertUserMappedToDto(User user, GetUserDto dto)
    {
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.IdentityId, dto.IdentityId);
        Assert.Equal(user.Nickname, dto.Nickname);
        Assert.Equal(user.CityId, dto.CityId);
        Assert.Equal((int)user.AccessStatus, dto.AccessStatus);
        Assert.Equal(user.CreatedAt, dto.CreatedAt);
        Assert.Equal(user.UpdatedAt, dto.UpdatedAt);
    }

    [Fact]
    public void Map_UserToGetUserDto_MapsAllPropertiesCorrectly()
    {
        var config = BuildIsolatedConfig();
        var user = UserFakers.Users().Generate();

        var dto = user.Adapt<GetUserDto>(config);

        AssertUserMappedToDto(user, dto);
    }

    [Theory]
    [MemberData(nameof(UserMappingConfigTestData.AccessStatuses), MemberType = typeof(UserMappingConfigTestData))]
    public void Map_UserToGetUserDto_MapsAccessStatusAsUnderlyingInt(UserAccessStatus accessStatus)
    {
        var config = BuildIsolatedConfig();
        var user = UserFakers.Users().Generate();
        user.AccessStatus = accessStatus;

        var dto = user.Adapt<GetUserDto>(config);

        Assert.Equal((int)accessStatus, dto.AccessStatus);
    }

    [Fact]
    public void Map_CreateUserDtoToUser_MapsIdentityIdAndNickname()
    {
        var config = BuildIsolatedConfig();
        var createUserDto = UserFakers.CreateUserDtos().Generate();

        var user = createUserDto.Adapt<User>(config);

        Assert.Equal(createUserDto.IdentityId, user.IdentityId);
        Assert.Equal(createUserDto.Nickname, user.Nickname);
    }

    [Fact]
    public void Map_CreateUserDtoToUser_LeavesAccessStatusUndefined()
    {
        var config = BuildIsolatedConfig();
        var createUserDto = UserFakers.CreateUserDtos().Generate();

        var user = createUserDto.Adapt<User>(config);

        Assert.Equal(UserAccessStatus.Undefined, user.AccessStatus);
    }

    [Fact]
    public void Map_CreateUserDtoToUser_IgnoresIdAndTimestamps()
    {
        var config = BuildIsolatedConfig();
        var createUserDto = UserFakers.CreateUserDtos().Generate();

        var user = createUserDto.Adapt<User>(config);

        Assert.Equal(Guid.Empty, user.Id);
        Assert.Equal(default, user.CreatedAt);
        Assert.Equal(default, user.UpdatedAt);
    }

    [Fact]
    public void Map_UserFilterDtoToUserFilter_MapsAllProperties()
    {
        var config = BuildIsolatedConfig();
        var filterDto = UserFakers.UserFilterDtos().Generate();

        var filter = filterDto.Adapt<UserFilter>(config);

        Assert.Equal(filterDto.Page, filter.Page);
        Assert.Equal(filterDto.PageSize, filter.PageSize);
        Assert.Equal(filterDto.Nickname, filter.Nickname);
        Assert.Equal(filterDto.CityId, filter.CityId);
    }

    [Fact]
    public void Map_PagedListOfUserToPagedListOfGetUserDto_MapsItemsAndPaginationFields()
    {
        const int itemCount = 3;
        const int page = 2;
        const int pageSize = 3;
        const int totalCount = 9;

        var config = BuildIsolatedConfig();
        var users = UserFakers.Users().Generate(itemCount);
        var pagedUsers = new PagedList<User>(users, page: page, pageSize: pageSize, totalCount: totalCount);

        var pagedDtos = pagedUsers.Adapt<PagedList<GetUserDto>>(config);

        Assert.Equal(pagedUsers.Page, pagedDtos.Page);
        Assert.Equal(pagedUsers.PageSize, pagedDtos.PageSize);
        Assert.Equal(pagedUsers.TotalCount, pagedDtos.TotalCount);

        Assert.Collection(
            pagedDtos.Items,
            dto => AssertUserMappedToDto(users[0], dto),
            dto => AssertUserMappedToDto(users[1], dto),
            dto => AssertUserMappedToDto(users[2], dto)
        );
    }
}
