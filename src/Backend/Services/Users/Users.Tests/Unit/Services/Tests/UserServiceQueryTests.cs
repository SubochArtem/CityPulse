using CityPulse.Contracts.Querying.Pagination;
using FluentValidation;
using Moq;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.DataAccess.Entities;
using Users.DataAccess.Models;
using Users.Tests.TestData;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class UserServiceQueryTests : UserServiceTestsBase
{
    private const string NonExistentIdentityId = "auth0|does-not-exist";

    [Fact]
    public async Task GetUserByIdAsync_UserDoesNotExist_ThrowsUserNotFoundException()
    {
        var userId = Guid.NewGuid();

        UserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => Sut.GetUserByIdAsync(userId));
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsMappedDto()
    {
        var user = UserFakers.Users().Generate();

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await Sut.GetUserByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.IdentityId, result.IdentityId);
    }

    [Fact]
    public async Task GetUserByIdentityIdAsync_UserDoesNotExist_ReturnsNullWithoutThrowing()
    {
        UserRepository
            .Setup(x => x.GetByIdentityIdAsync(NonExistentIdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await Sut.GetUserByIdentityIdAsync(NonExistentIdentityId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdentityIdAsync_UserExists_ReturnsMappedDto()
    {
        var user = UserFakers.Users().Generate();

        UserRepository
            .Setup(x => x.GetByIdentityIdAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await Sut.GetUserByIdentityIdAsync(user.IdentityId);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.IdentityId, result.IdentityId);
    }

    [Fact]
    public async Task GetUsersAsync_MapsFilterAndReturnsPagedResults()
    {
        var filterDto = UserFakers.UserFilterDtos().Generate();
        var users = UserFakers.Users().Generate(2);
        var pagedUsers = new PagedList<User>(users, filterDto.Page, filterDto.PageSize, totalCount: 2);
        UserFilter? capturedFilter = null;

        UserRepository
            .Setup(x => x.GetFilteredAsync(It.IsAny<UserFilter>(), It.IsAny<CancellationToken>()))
            .Callback<UserFilter, CancellationToken>((filter, _) => capturedFilter = filter)
            .ReturnsAsync(pagedUsers);

        var result = await Sut.GetUsersAsync(filterDto);

        Assert.NotNull(capturedFilter);
        Assert.Equal(filterDto.Page, capturedFilter!.Page);
        Assert.Equal(filterDto.PageSize, capturedFilter.PageSize);
        Assert.Equal(filterDto.Nickname, capturedFilter.Nickname);
        Assert.Equal(filterDto.CityId, capturedFilter.CityId);
        Assert.Equal(users.Count, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetPublicProfilesAsync_UsersExist_ReturnsOnlyIdAndNickname()
    {
        var users = UserFakers.Users().Generate(2);
        var request = new PublicProfilesRequestDto { Ids = users.Select(x => x.Id).ToArray() };

        UserRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await Sut.GetPublicProfilesAsync(request);

        Assert.Equal(users.Count, result.Count);
        foreach (var user in users)
        {
            var profile = Assert.Single(result, x => x.Id == user.Id);
            Assert.Equal(user.Nickname, profile.Nickname);
        }
    }

    [Fact]
    public async Task GetPublicProfilesAsync_DuplicateIds_QueriesRepositoryWithDistinctIds()
    {
        var id = Guid.NewGuid();
        var request = new PublicProfilesRequestDto { Ids = [id, id, id] };
        IReadOnlyCollection<Guid>? capturedIds = null;

        UserRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<Guid>, CancellationToken>((ids, _) => capturedIds = ids)
            .ReturnsAsync([]);

        await Sut.GetPublicProfilesAsync(request);

        Assert.NotNull(capturedIds);
        Assert.Equal(id, Assert.Single(capturedIds!));
    }

    [Fact]
    public async Task GetPublicProfilesAsync_NoUserMatches_ReturnsEmptyList()
    {
        var request = new PublicProfilesRequestDto { Ids = [Guid.NewGuid()] };

        UserRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await Sut.GetPublicProfilesAsync(request);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ValidationConstants.PublicProfiles.MaxIds + 1)]
    public async Task GetPublicProfilesAsync_InvalidIdsCount_ThrowsValidationExceptionAndDoesNotCallRepository(
        int count)
    {
        var request = new PublicProfilesRequestDto
        {
            Ids = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray()
        };

        await Assert.ThrowsAsync<ValidationException>(() => Sut.GetPublicProfilesAsync(request));

        UserRepository.Verify(
            x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUsersAsync_FilterHasInvalidPage_ThrowsValidationExceptionAndDoesNotCallRepository()
    {
        const int invalidPage = 0;
        const int oversizedPageSize = 999;

        var filterDto = new UserFilterDto
        {
            Page = invalidPage,
            PageSize = oversizedPageSize,
            Nickname = null,
            CityId = null
        };

        await Assert.ThrowsAsync<ValidationException>(() => Sut.GetUsersAsync(filterDto));

        UserRepository.Verify(
            x => x.GetFilteredAsync(It.IsAny<UserFilter>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
