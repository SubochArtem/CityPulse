using Moq;
using Users.Business.Exceptions;
using Users.DataAccess.Entities;
using Users.Tests.TestData;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class UserServiceDeleteTests : UserServiceTestsBase
{


    [Fact]
    public async Task DeleteUserAsync_UserDoesNotExist_ThrowsUserNotFoundExceptionAndCallsNothingElse()
    {
        var userId = Guid.NewGuid();

        UserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => Sut.DeleteUserAsync(userId));

        IdentityProvider.Verify(
            x => x.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_UserExists_DeletesFromIdentityProviderAndRepository()
    {
        var user = UserFakers.Users().Generate();

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.DeleteUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        UserRepository
            .Setup(x => x.DeleteAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Sut.DeleteUserAsync(user.Id);

        IdentityProvider.Verify(
            x => x.DeleteUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepository.Verify(
            x => x.DeleteAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_IdentityProviderFails_DoesNotDeleteFromRepository()
    {
        var user = UserFakers.Users().Generate();

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.DeleteUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(Auth0UnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.DeleteUserAsync(user.Id));

        UserRepository.Verify(
            x => x.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
