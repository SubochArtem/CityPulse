using CityPulse.Contracts.Events.Users;
using CityPulse.Contracts.Events.Users.Enums;
using CityPulse.Contracts.Querying.Pagination;
using Mapster;
using MapsterMapper;
using Moq;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Interfaces;
using Users.Business.Mapping;
using Users.Business.Services;
using Users.Business.Validators;
using Users.DataAccess.Entities;
using Users.DataAccess.Entities.Enums;
using Users.DataAccess.Interfaces;
using Users.DataAccess.Models;
using Users.Tests.TestData;
using Xunit;

namespace Users.Tests.Unit.Services;

public sealed class UserServiceTests
{
    private const string NonExistentIdentityId = "auth0|does-not-exist";
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
        config.Compile();
        _mapper = new Mapper(config);

        _sut = new UserService(
            _userRepository.Object,
            _identityProvider.Object,
            new CreateUserValidator(),
            new UpdateUserProfileDtoValidator(),
            _cityService.Object,
            _mapper,
            _eventPublisher.Object);
    }

    [Fact]
    public async Task ActivateUserAsync_UserDoesNotExist_ThrowsUserNotFoundExceptionAndCallsNothingElse()
    {
        var userId = Guid.NewGuid();

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.ActivateUserAsync(userId));

        _identityProvider.Verify(
            x => x.UnblockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _eventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActivateUserAsync_UserIsAlreadyActive_IsNoOp()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Active;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.ActivateUserAsync(user.Id);

        _identityProvider.Verify(
            x => x.UnblockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _eventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActivateUserAsync_UserIsInactive_UnblocksPublishesAndPersistsActiveStatus()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;
        UserStatusChangedEvent? capturedEvent = null;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _eventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserStatusChangedEvent, CancellationToken>((evt, _) => capturedEvent = evt)
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.ActivateUserAsync(user.Id);

        Assert.NotNull(capturedEvent);
        Assert.Equal(user.Id, capturedEvent!.UserId);
        Assert.Equal(UserLifecycleStatus.Active, capturedEvent.UserLifecycleStatus);
        _userRepository.Verify(
            x => x.UpdateAsync(It.Is<User>(u => u.AccessStatus == UserAccessStatus.Active), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivateUserAsync_UserStatusIsUndefined_TreatsAsStateChangeNotNoOp()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Undefined;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _eventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.ActivateUserAsync(user.Id);

        _identityProvider.Verify(
            x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        _eventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivateUserAsync_IdentityProviderFails_DoesNotPublishOrPersist()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(Auth0UnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ActivateUserAsync(user.Id));

        _eventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActivateUserAsync_EventPublishFails_IdentityProviderAlreadyCalledButRepositoryNotUpdated()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _eventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(BusUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ActivateUserAsync(user.Id));

        _identityProvider.Verify(
            x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActivateUserAsync_RepositoryUpdateFails_IdentityProviderAndEventAlreadyHappened()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _eventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(DbUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ActivateUserAsync(user.Id));

        _identityProvider.Verify(
            x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        _eventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateUserAsync_UserDoesNotExist_ThrowsUserNotFoundExceptionAndCallsNothingElse()
    {
        var userId = Guid.NewGuid();

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.DeactivateUserAsync(userId));

        _identityProvider.Verify(
            x => x.BlockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _eventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateUserAsync_UserIsAlreadyInactive_IsNoOp()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.DeactivateUserAsync(user.Id);

        _identityProvider.Verify(
            x => x.BlockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _eventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [MemberData(nameof(NonInactiveStatuses))]
    public async Task DeactivateUserAsync_UserIsNotInactive_BlocksPublishesAndPersistsInactiveStatus(UserAccessStatus initialStatus)
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = initialStatus;
        UserStatusChangedEvent? capturedEvent = null;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _eventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserStatusChangedEvent, CancellationToken>((evt, _) => capturedEvent = evt)
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeactivateUserAsync(user.Id);

        Assert.NotNull(capturedEvent);
        Assert.Equal(user.Id, capturedEvent!.UserId);
        Assert.Equal(UserLifecycleStatus.Inactive, capturedEvent.UserLifecycleStatus);
        _userRepository.Verify(
            x => x.UpdateAsync(It.Is<User>(u => u.AccessStatus == UserAccessStatus.Inactive), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateUserAsync_IdentityProviderFails_DoesNotPublishOrPersist()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Active;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(Auth0UnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeactivateUserAsync(user.Id));

        _eventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateUserAsync_EventPublishFails_IdentityProviderAlreadyCalledButRepositoryNotUpdated()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Active;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _eventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(BusUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeactivateUserAsync(user.Id));

        _identityProvider.Verify(
            x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateUserAsync_RepositoryUpdateFails_IdentityProviderAndEventAlreadyHappened()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Active;

        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _identityProvider
            .Setup(x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _eventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(DbUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeactivateUserAsync(user.Id));

        _identityProvider.Verify(
            x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        _eventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

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

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.IdentityId, result.IdentityId);
        Assert.Equal(user.Nickname, result.Nickname);
        Assert.Equal(user.CityId, result.CityId);
        Assert.Equal((int)user.AccessStatus, result.AccessStatus);
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
    public async Task GetUsersAsync_FilterHasInvalidPage_DoesNotThrowValidationException()
    {
        const int invalidPage = 0;
        const int oversizedPageSize = 999;
        const int expectedTotalCount = 0;

        var filterDto = new UserFilterDto 
        { 
            Page = invalidPage, 
            PageSize = oversizedPageSize, 
            Nickname = null, 
            CityId = null 
        };
        var pagedUsers = new PagedList<User>([], invalidPage, oversizedPageSize, expectedTotalCount);

        _userRepository
            .Setup(x => x.GetFilteredAsync(It.IsAny<UserFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedUsers);

        var result = await _sut.GetUsersAsync(filterDto);

        Assert.Empty(result.Items);
    }

    public static TheoryData<UserAccessStatus> NonInactiveStatuses => new()
    {
        UserAccessStatus.Active,
        UserAccessStatus.Undefined
    };
}
