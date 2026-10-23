using CityPulse.Contracts.Querying.Pagination;
using FluentValidation;
using Moq;
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
