using FluentValidation;
using Moq;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.DataAccess.Entities;
using Users.Tests.TestData;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class UserServiceCreateTests : UserServiceTestsBase
{
    private const string InvalidIdentityId = "auth0";

    [Fact]
    public async Task CreateUserAsync_NewUser_CreatesMappedUserAndReturnsDto()
    {
        var createDto = UserFakers.CreateUserDtos().Generate();
        User? createdUser = null;

        UserRepository
            .Setup(x => x.GetByIdentityIdAsync(createDto.IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        UserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .Returns(Task.CompletedTask);

        var result = await Sut.CreateUserAsync(createDto);

        Assert.NotNull(createdUser);
        Assert.Equal(createDto.IdentityId, createdUser.IdentityId);
        Assert.Equal(createDto.Nickname, createdUser.Nickname);
        Assert.Equal(createDto.IdentityId, result.IdentityId);
        Assert.Equal(createDto.Nickname, result.Nickname);
    }

    [Fact]
    public async Task CreateUserAsync_InvalidDto_ThrowsValidationExceptionAndDoesNotCallRepository()
    {
        var createDto = new CreateUserDto
        {
            IdentityId = InvalidIdentityId,
            Nickname = UserFakers.CreateUserDtos().Generate().Nickname
        };

        await Assert.ThrowsAsync<ValidationException>(() => Sut.CreateUserAsync(createDto));

        UserRepository.Verify(
            x => x.GetByIdentityIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_IdentityIdAlreadyExists_ThrowsUserAlreadyExistsExceptionAndDoesNotCreate()
    {
        var createDto = UserFakers.CreateUserDtos().Generate();
        var existingUser = UserFakers.Users().Generate();

        UserRepository
            .Setup(x => x.GetByIdentityIdAsync(createDto.IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var exception = await Assert.ThrowsAsync<UserAlreadyExistsException>(() => Sut.CreateUserAsync(createDto));

        Assert.Equal(createDto.IdentityId, exception.IdentityId);
        Assert.Equal(IdentitySources.Auth0, exception.IdentityProvider);
        UserRepository.Verify(
            x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_RepositoryCreateFails_PropagatesException()
    {
        var createDto = UserFakers.CreateUserDtos().Generate();

        UserRepository
            .Setup(x => x.GetByIdentityIdAsync(createDto.IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        UserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(DbUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.CreateUserAsync(createDto));
    }
}
