using CityPulse.Contracts.Events.Users;
using CityPulse.Contracts.Events.Users.Enums;
using CityPulse.Contracts.Querying.Pagination;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Moq;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Interfaces;
using Users.Business.Mapping;
using Users.Business.Services;
using Users.Business.Validators;
using Users.DataAccess.DTOs;
using Users.DataAccess.Entities;
using Users.DataAccess.Entities.Enums;
using Users.DataAccess.Interfaces;
using Users.DataAccess.Models;
using Users.Tests.TestData;
using Users.Tests.Unit.Services.TestData;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class UserServiceTests
{
    private const string NonExistentIdentityId = "auth0|does-not-exist";
    private const string InvalidIdentityId = "auth0";
    private const string NewNickname = "new_nickname";
    private const string CityName = "Test City";
    private const string CityServiceUnavailableMessage = "city service unavailable";
    private const string Auth0UnavailableMessage = "Auth0 unavailable";
    private const string BusUnavailableMessage = "bus unavailable";
    private const string DbUnavailableMessage = "db unavailable";

    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
    private readonly Mock<IIdentityProvider> _identityProvider = new(MockBehavior.Strict);
    private readonly Mock<ICityService> _cityService = new(MockBehavior.Strict);
    private readonly Mock<IEventPublisher> _eventPublisher = new(MockBehavior.Strict);
    private readonly IMapper _mapper;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        var config = new TypeAdapterConfig();
        UserMappingConfig.Configure(config);
        _mapper = new Mapper(config);

        _sut = new UserService(
            _userRepository.Object,
            _identityProvider.Object,
            new UserValidators(
                new CreateUserValidator(),
                new UpdateUserProfileDtoValidator(),
                new UserFilterDtoValidator()),
            _cityService.Object,
            _mapper,
            _eventPublisher.Object);
    }

    private static CityDto BuildCity(Guid cityId, CityStatus status) => new()
    {
        Id = cityId,
        Name = CityName,
        Status = status
    };

    [Fact]
    public async Task DeleteUserAsync_UserDoesNotExist_ThrowsUserNotFoundExceptionAndCallsNothingElse()
    {
        var userId = Guid.NewGuid();

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.DeleteUserAsync(userId));

        _identityProvider.Verify(
            x => x.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            x => x.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_UserExists_DeletesFromIdentityProviderAndRepository()
    {
        var user = UserFakers.Users().Generate();

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.DeleteUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(x => x.DeleteAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteUserAsync(user.Id);

        _identityProvider.Verify(
            x => x.DeleteUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepository.Verify(
            x => x.DeleteAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_IdentityProviderFails_DoesNotDeleteFromRepository()
    {
        var user = UserFakers.Users().Generate();

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.DeleteUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(Auth0UnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteUserAsync(user.Id));

        _userRepository.Verify(
            x => x.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserDoesNotExist_ThrowsUserNotFoundException()
    {
        var userId = Guid.NewGuid();

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.GetUserByIdAsync(userId));
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsMappedDto()
    {
        var user = UserFakers.Users().Generate();

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetUserByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.IdentityId, result.IdentityId);
    }

    [Fact]
    public async Task GetUserByIdentityIdAsync_UserDoesNotExist_ReturnsNullWithoutThrowing()
    {
        _userRepository
            .Setup(x => x.GetByIdentityIdAsync(NonExistentIdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetUserByIdentityIdAsync(NonExistentIdentityId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdentityIdAsync_UserExists_ReturnsMappedDto()
    {
        var user = UserFakers.Users().Generate();

        _userRepository
            .Setup(x => x.GetByIdentityIdAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetUserByIdentityIdAsync(user.IdentityId);

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

        _userRepository
            .Setup(x => x.GetFilteredAsync(It.IsAny<UserFilter>(), It.IsAny<CancellationToken>()))
            .Callback<UserFilter, CancellationToken>((filter, _) => capturedFilter = filter)
            .ReturnsAsync(pagedUsers);

        var result = await _sut.GetUsersAsync(filterDto);

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

        await Assert.ThrowsAsync<ValidationException>(() => _sut.GetUsersAsync(filterDto));

        _userRepository.Verify(
            x => x.GetFilteredAsync(It.IsAny<UserFilter>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

}
