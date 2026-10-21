using System.Net;
using System.Net.Http.Json;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.DataAccess.DTOs;
using Users.Presentation.Middleware;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Users.TestData;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class UpdateUserErrorHandlingTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";

    [Theory]
    [MemberData(
        nameof(UpdateUserErrorHandlingTestData.NotActiveCityStatuses),
        MemberType = typeof(UpdateUserErrorHandlingTestData))]
    public async Task UpdateUser_CityIsNotActive_ReturnsUnprocessableEntityAndDoesNotChangeUser(CityStatus status)
    {
        var user = await SeedUserAsync();
        var cityId = Guid.NewGuid();
        SetupCity(cityId, status);

        var response = await Client.PatchAsJsonAsync(
            $"{UsersRoute}/{user.Id}",
            new UpdateUserProfileDto { CityId = cityId });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.CityNotActive, problem.Title);

        await AssertUserUnchangedAsync(user.Id, user.Nickname, user.CityId);
        VerifyIdentityProviderNeverUpdated();
    }

    [Theory]
    [MemberData(
        nameof(UpdateUserErrorHandlingTestData.CitiesServiceFailures),
        MemberType = typeof(UpdateUserErrorHandlingTestData))]
    public async Task UpdateUser_CitiesServiceFails_ReturnsMappedErrorAndDoesNotChangeUser(
        StatusCode grpcStatusCode,
        HttpStatusCode expectedHttpStatusCode,
        string expectedTitle)
    {
        var user = await SeedUserAsync();
        var cityId = Guid.NewGuid();
        CityServiceMock
            .Setup(x => x.GetCityAsync(cityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RpcException(new Status(grpcStatusCode, string.Empty)));

        var response = await Client.PatchAsJsonAsync(
            $"{UsersRoute}/{user.Id}",
            new UpdateUserProfileDto { CityId = cityId });

        Assert.Equal(expectedHttpStatusCode, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(expectedTitle, problem.Title);

        await AssertUserUnchangedAsync(user.Id, user.Nickname, user.CityId);
        VerifyIdentityProviderNeverUpdated();
    }

    [Fact]
    public async Task UpdateUser_IdentityProviderFails_ReturnsBadGatewayAndDoesNotChangeUser()
    {
        var user = await SeedUserAsync();
        IdentityProviderMock
            .Setup(x => x.UpdateUserProfileAsync(
                user.IdentityId,
                It.IsAny<UpdateUserProfileDto>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Auth0Exception(UpdateUserErrorHandlingTestData.Auth0FailureMessage));

        var response = await Client.PatchAsJsonAsync(
            $"{UsersRoute}/{user.Id}",
            new UpdateUserProfileDto { Nickname = UpdateUserErrorHandlingTestData.NewNickname });

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.IdentityProviderError, problem.Title);
        Assert.Equal(MiddlewareExceptionMessages.Details.IdentityProviderCommunicationError, problem.Detail);

        await AssertUserUnchangedAsync(user.Id, user.Nickname, user.CityId);
    }

    private async Task AssertUserUnchangedAsync(Guid userId, string nickname, Guid? cityId)
    {
        var unchanged = await GetUserFromDbAsync(userId);

        Assert.NotNull(unchanged);
        Assert.Equal(nickname, unchanged.Nickname);
        Assert.Equal(cityId, unchanged.CityId);
    }
}
