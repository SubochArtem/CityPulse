using CityPulse.Contracts.Events.Users;
using CityPulse.Contracts.Events.Users.Enums;
using Moq;
using Users.Business.Exceptions;
using Users.DataAccess.Entities;
using Users.DataAccess.Entities.Enums;
using Users.Tests.TestData;
using Users.Tests.Unit.Services.TestData;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class UserServiceActivationTests : UserServiceTestsBase
{
    private const string BusUnavailableMessage = "bus unavailable";

    [Fact]
    public async Task ActivateUserAsync_UserDoesNotExist_ThrowsUserNotFoundExceptionAndCallsNothingElse()
    {
        var userId = Guid.NewGuid();

        UserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => Sut.ActivateUserAsync(userId));

        IdentityProvider.Verify(
            x => x.UnblockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActivateUserAsync_UserIsAlreadyActive_IsNoOp()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Active;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Sut.ActivateUserAsync(user.Id);

        IdentityProvider.Verify(
            x => x.UnblockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActivateUserAsync_UserIsInactive_UnblocksPublishesAndPersistsActiveStatus()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;
        UserStatusChangedEvent? capturedEvent = null;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        EventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserStatusChangedEvent, CancellationToken>((evt, _) => capturedEvent = evt)
            .Returns(Task.CompletedTask);
        UserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Sut.ActivateUserAsync(user.Id);

        Assert.NotNull(capturedEvent);
        Assert.Equal(user.Id, capturedEvent!.UserId);
        Assert.Equal(UserLifecycleStatus.Active, capturedEvent.UserLifecycleStatus);
        UserRepository.Verify(
            x => x.UpdateAsync(It.Is<User>(u => u.AccessStatus == UserAccessStatus.Active), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivateUserAsync_UserStatusIsUndefined_TreatsAsStateChangeNotNoOp()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Undefined;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        EventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        UserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Sut.ActivateUserAsync(user.Id);

        IdentityProvider.Verify(
            x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepository.Verify(
            x => x.UpdateAsync(It.Is<User>(u => u.AccessStatus == UserAccessStatus.Active), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivateUserAsync_IdentityProviderFails_DoesNotPublishOrPersist()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(Auth0UnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.ActivateUserAsync(user.Id));

        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActivateUserAsync_EventPublishFails_IdentityProviderAlreadyCalledButRepositoryNotUpdated()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        EventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(BusUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.ActivateUserAsync(user.Id));

        IdentityProvider.Verify(
            x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActivateUserAsync_RepositoryUpdateFails_IdentityProviderAndEventAlreadyHappened()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        EventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        UserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(DbUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.ActivateUserAsync(user.Id));

        IdentityProvider.Verify(
            x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateUserAsync_UserDoesNotExist_ThrowsUserNotFoundExceptionAndCallsNothingElse()
    {
        var userId = Guid.NewGuid();

        UserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => Sut.DeactivateUserAsync(userId));

        IdentityProvider.Verify(
            x => x.BlockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateUserAsync_UserIsAlreadyInactive_IsNoOp()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Inactive;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Sut.DeactivateUserAsync(user.Id);

        IdentityProvider.Verify(
            x => x.BlockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [MemberData(nameof(UserServiceTestData.NonInactiveStatuses), MemberType = typeof(UserServiceTestData))]
    public async Task DeactivateUserAsync_UserIsNotInactive_BlocksPublishesAndPersistsInactiveStatus(UserAccessStatus initialStatus)
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = initialStatus;
        UserStatusChangedEvent? capturedEvent = null;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        EventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserStatusChangedEvent, CancellationToken>((evt, _) => capturedEvent = evt)
            .Returns(Task.CompletedTask);
        UserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Sut.DeactivateUserAsync(user.Id);

        Assert.NotNull(capturedEvent);
        Assert.Equal(user.Id, capturedEvent!.UserId);
        Assert.Equal(UserLifecycleStatus.Inactive, capturedEvent.UserLifecycleStatus);
        UserRepository.Verify(
            x => x.UpdateAsync(It.Is<User>(u => u.AccessStatus == UserAccessStatus.Inactive), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateUserAsync_IdentityProviderFails_DoesNotPublishOrPersist()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Active;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(Auth0UnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.DeactivateUserAsync(user.Id));

        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateUserAsync_EventPublishFails_IdentityProviderAlreadyCalledButRepositoryNotUpdated()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Active;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        EventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(BusUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.DeactivateUserAsync(user.Id));

        IdentityProvider.Verify(
            x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateUserAsync_RepositoryUpdateFails_IdentityProviderAndEventAlreadyHappened()
    {
        var user = UserFakers.Users().Generate();
        user.AccessStatus = UserAccessStatus.Active;

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        EventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        UserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(DbUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.DeactivateUserAsync(user.Id));

        IdentityProvider.Verify(
            x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<UserStatusChangedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
