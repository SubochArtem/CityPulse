using FluentValidation;
using Moq;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.DataAccess.DTOs;
using Users.DataAccess.Entities;
using Users.Tests.TestData;
using Users.Tests.Unit.Services.TestData;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class UserServiceUpdateTests : UserServiceTestsBase
{
    private const string NewNickname = "new_nickname";
    private const string CityName = "Test City";
    private const string CityServiceUnavailableMessage = "city service unavailable";

    private static CityDto BuildCity(Guid cityId, CityStatus status) => new()
    {
        Id = cityId,
        Name = CityName,
        Status = status
    };

    [Fact]
    public async Task UpdateUserAsync_InvalidDto_ThrowsValidationExceptionAndCallsNothingElse()
    {
        var updateDto = new UpdateUserProfileDto { Nickname = string.Empty };

        await Assert.ThrowsAsync<ValidationException>(() => Sut.UpdateUserAsync(Guid.NewGuid(), updateDto));

        UserRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        IdentityProvider.Verify(
            x => x.UpdateUserProfileAsync(
                It.IsAny<string>(),
                It.IsAny<UpdateUserProfileDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_UserDoesNotExist_ThrowsUserNotFoundExceptionAndCallsNothingElse()
    {
        var userId = Guid.NewGuid();
        var updateDto = UserFakers.UpdateUserProfileDtos().Generate();

        UserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() => Sut.UpdateUserAsync(userId, updateDto));

        CityService.Verify(
            x => x.GetCityAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        IdentityProvider.Verify(
            x => x.UpdateUserProfileAsync(
                It.IsAny<string>(),
                It.IsAny<UpdateUserProfileDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_NicknameOnly_UpdatesNicknameInIdentityProviderAndRepositoryWithoutCityLookup()
    {
        var user = UserFakers.Users().Generate();
        var originalCityId = user.CityId;
        var updateDto = new UpdateUserProfileDto { Nickname = NewNickname };

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.UpdateUserProfileAsync(user.IdentityId, updateDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        UserRepository
            .Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Sut.UpdateUserAsync(user.Id, updateDto);

        Assert.Equal(NewNickname, user.Nickname);
        Assert.Equal(originalCityId, user.CityId);
        Assert.Equal(NewNickname, result.Nickname);
        CityService.Verify(
            x => x.GetCityAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        IdentityProvider.Verify(
            x => x.UpdateUserProfileAsync(user.IdentityId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ActiveCityOnly_UpdatesCityInIdentityProviderAndRepositoryWithoutChangingNickname()
    {
        var user = UserFakers.Users().Generate();
        var originalNickname = user.Nickname;
        var cityId = Guid.NewGuid();
        var updateDto = new UpdateUserProfileDto { CityId = cityId };

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        CityService
            .Setup(x => x.GetCityAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCity(cityId, CityStatus.Active));
        IdentityProvider
            .Setup(x => x.UpdateUserProfileAsync(user.IdentityId, updateDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        UserRepository
            .Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Sut.UpdateUserAsync(user.Id, updateDto);

        Assert.Equal(cityId, user.CityId);
        Assert.Equal(originalNickname, user.Nickname);
        Assert.Equal(cityId, result.CityId);
        IdentityProvider.Verify(
            x => x.UpdateUserProfileAsync(user.IdentityId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
        UserRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_NicknameAndActiveCity_UpdatesBothFields()
    {
        var user = UserFakers.Users().Generate();
        var cityId = Guid.NewGuid();
        var updateDto = new UpdateUserProfileDto { Nickname = NewNickname, CityId = cityId };

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        CityService
            .Setup(x => x.GetCityAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCity(cityId, CityStatus.Active));
        IdentityProvider
            .Setup(x => x.UpdateUserProfileAsync(user.IdentityId, updateDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        UserRepository
            .Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Sut.UpdateUserAsync(user.Id, updateDto);

        Assert.Equal(NewNickname, user.Nickname);
        Assert.Equal(cityId, user.CityId);
        Assert.Equal(NewNickname, result.Nickname);
        Assert.Equal(cityId, result.CityId);
    }

    [Theory]
    [MemberData(nameof(UserServiceTestData.NonActiveCityStatuses), MemberType = typeof(UserServiceTestData))]
    public async Task UpdateUserAsync_CityIsNotActive_ThrowsCityNotActiveExceptionAndDoesNotPersist(CityStatus cityStatus)
    {
        var user = UserFakers.Users().Generate();
        var originalCityId = user.CityId;
        var cityId = Guid.NewGuid();
        var updateDto = new UpdateUserProfileDto { Nickname = NewNickname, CityId = cityId };

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        CityService
            .Setup(x => x.GetCityAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCity(cityId, cityStatus));

        await Assert.ThrowsAsync<CityNotActiveException>(() => Sut.UpdateUserAsync(user.Id, updateDto));

        Assert.Equal(originalCityId, user.CityId);
        IdentityProvider.Verify(
            x => x.UpdateUserProfileAsync(
                It.IsAny<string>(),
                It.IsAny<UpdateUserProfileDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_NothingToUpdate_SkipsIdentityProviderAndPersistsUser()
    {
        var user = UserFakers.Users().Generate();

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        UserRepository
            .Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Sut.UpdateUserAsync(user.Id, new UpdateUserProfileDto());

        CityService.Verify(
            x => x.GetCityAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        IdentityProvider.Verify(
            x => x.UpdateUserProfileAsync(
                It.IsAny<string>(),
                It.IsAny<UpdateUserProfileDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_CityServiceFails_DoesNotCallIdentityProviderOrRepository()
    {
        var user = UserFakers.Users().Generate();
        var cityId = Guid.NewGuid();
        var updateDto = new UpdateUserProfileDto { CityId = cityId };

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        CityService
            .Setup(x => x.GetCityAsync(cityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(CityServiceUnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.UpdateUserAsync(user.Id, updateDto));

        IdentityProvider.Verify(
            x => x.UpdateUserProfileAsync(
                It.IsAny<string>(),
                It.IsAny<UpdateUserProfileDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_IdentityProviderFails_DoesNotPersistUser()
    {
        var user = UserFakers.Users().Generate();
        var updateDto = new UpdateUserProfileDto { Nickname = NewNickname };

        UserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        IdentityProvider
            .Setup(x => x.UpdateUserProfileAsync(user.IdentityId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(Auth0UnavailableMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.UpdateUserAsync(user.Id, updateDto));

        UserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
