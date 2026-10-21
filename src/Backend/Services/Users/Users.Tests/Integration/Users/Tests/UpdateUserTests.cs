using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Users.Business.DTOs;
using Users.DataAccess.DTOs;
using Users.Presentation.Middleware;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Users.TestData;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class UpdateUserTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";
    private const string JsonContentType = "application/json";
    private static readonly TimeSpan TimestampTolerance = TimeSpan.FromMilliseconds(1);

    [Fact]
    public async Task UpdateUser_NicknameOnly_UpdatesNicknameAndNotifiesIdentityProvider()
    {
        var user = await SeedUserAsync();
        var update = new UpdateUserProfileDto { Nickname = UpdateUserTestData.NewNickname };

        var response = await Client.PatchAsJsonAsync($"{UsersRoute}/{user.Id}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<GetUserDto>();
        Assert.NotNull(dto);
        Assert.Equal(UpdateUserTestData.NewNickname, dto.Nickname);
        Assert.Equal(user.CityId, dto.CityId);

        var updated = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal(UpdateUserTestData.NewNickname, updated.Nickname);
        Assert.Equal(user.CityId, updated.CityId);
        Assert.True(updated.UpdatedAt > user.UpdatedAt);
        Assert.True((updated.CreatedAt - user.CreatedAt).Duration() < TimestampTolerance);

        VerifyIdentityProviderUpdatedOnce(user.IdentityId, UpdateUserTestData.NewNickname, cityId: null);
        CityServiceMock.Verify(
            x => x.GetCityAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ActiveCityOnly_UpdatesCityAndNotifiesIdentityProvider()
    {
        var user = await SeedUserAsync();
        var cityId = Guid.NewGuid();
        SetupCity(cityId, CityStatus.Active);

        var response = await Client.PatchAsJsonAsync(
            $"{UsersRoute}/{user.Id}",
            new UpdateUserProfileDto { CityId = cityId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<GetUserDto>();
        Assert.NotNull(dto);
        Assert.Equal(cityId, dto.CityId);
        Assert.Equal(user.Nickname, dto.Nickname);

        var updated = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal(cityId, updated.CityId);
        Assert.Equal(user.Nickname, updated.Nickname);

        VerifyIdentityProviderUpdatedOnce(user.IdentityId, nickname: null, cityId);
    }

    [Fact]
    public async Task UpdateUser_NicknameAndCity_UpdatesBothFields()
    {
        var user = await SeedUserAsync();
        var cityId = Guid.NewGuid();
        SetupCity(cityId, CityStatus.Active);

        var response = await Client.PatchAsJsonAsync(
            $"{UsersRoute}/{user.Id}",
            new UpdateUserProfileDto { Nickname = UpdateUserTestData.NewNickname, CityId = cityId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal(UpdateUserTestData.NewNickname, updated.Nickname);
        Assert.Equal(cityId, updated.CityId);

        VerifyIdentityProviderUpdatedOnce(user.IdentityId, UpdateUserTestData.NewNickname, cityId);
    }

    [Theory]
    [MemberData(nameof(UpdateUserTestData.EmptyUpdateBodies), MemberType = typeof(UpdateUserTestData))]
    public async Task UpdateUser_NothingToUpdate_ReturnsOkWithoutChangesAndWithoutCallingIdentityProvider(string body)
    {
        var user = await SeedUserAsync();
        using var content = new StringContent(body, Encoding.UTF8, JsonContentType);

        var response = await Client.PatchAsync($"{UsersRoute}/{user.Id}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var unchanged = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(unchanged);
        Assert.Equal(user.Nickname, unchanged.Nickname);
        Assert.Equal(user.CityId, unchanged.CityId);

        IdentityProviderMock.Verify(
            x => x.UpdateUserProfileAsync(
                It.IsAny<string>(),
                It.IsAny<UpdateUserProfileDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUser_UserDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var response = await Client.PatchAsJsonAsync(
            $"{UsersRoute}/{Guid.NewGuid()}",
            new UpdateUserProfileDto { Nickname = UpdateUserTestData.NewNickname });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.UserNotFound, problem.Title);

        IdentityProviderMock.Verify(
            x => x.UpdateUserProfileAsync(
                It.IsAny<string>(),
                It.IsAny<UpdateUserProfileDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupCity(Guid cityId, CityStatus status) =>
        CityServiceMock
            .Setup(x => x.GetCityAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CityDto { Id = cityId, Name = UpdateUserTestData.CityName, Status = status });

    private void VerifyIdentityProviderUpdatedOnce(string identityId, string? nickname, Guid? cityId) =>
        IdentityProviderMock.Verify(
            x => x.UpdateUserProfileAsync(
                identityId,
                It.Is<UpdateUserProfileDto>(d => d.Nickname == nickname && d.CityId == cityId),
                It.IsAny<CancellationToken>()),
            Times.Once);
}
