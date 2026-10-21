using System.Net;
using System.Net.Http.Json;
using CityPulse.Contracts.Events.Users.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Users.Business.Exceptions;
using Users.DataAccess.Entities.Enums;
using Users.Presentation.Middleware;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Users.TestData;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class ActivateUserTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";
    private const string Auth0FailureMessage = "Auth0 is down";

    [Theory]
    [MemberData(
        nameof(ActivateUserTestData.StatusesThatCanBeActivated),
        MemberType = typeof(ActivateUserTestData))]
    public async Task ActivateUser_UserIsNotActive_UnblocksUserSavesStatusAndPublishesEvent(UserAccessStatus status)
    {
        var user = await SeedUserAsync(status);

        var response = await Client.PostAsync($"{UsersRoute}/{user.Id}/activate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal(UserAccessStatus.Active, updated.AccessStatus);
        Assert.True(updated.UpdatedAt > user.UpdatedAt);

        IdentityProviderMock.Verify(
            x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);

        var published = Assert.Single(await GetOutboxStatusChangedEventsAsync());
        Assert.Equal(user.Id, published.UserId);
        Assert.Equal(UserLifecycleStatus.Active, published.UserLifecycleStatus);
    }

    [Fact]
    public async Task ActivateUser_UserIsAlreadyActive_ReturnsOkAndDoesNothing()
    {
        var user = await SeedUserAsync(UserAccessStatus.Active);

        var response = await Client.PostAsync($"{UsersRoute}/{user.Id}/activate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var unchanged = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(unchanged);
        Assert.Equal(UserAccessStatus.Active, unchanged.AccessStatus);

        VerifyIdentityProviderNeverUnblocked();
        Assert.Empty(await GetOutboxStatusChangedEventsAsync());
    }

    [Fact]
    public async Task ActivateUser_UserDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var response = await Client.PostAsync($"{UsersRoute}/{Guid.NewGuid()}/activate", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.UserNotFound, problem.Title);

        VerifyIdentityProviderNeverUnblocked();
        Assert.Empty(await GetOutboxStatusChangedEventsAsync());
    }

    [Fact]
    public async Task ActivateUser_IdentityProviderFails_ReturnsBadGatewayAndKeepsUserInactive()
    {
        var user = await SeedUserAsync(UserAccessStatus.Inactive);
        IdentityProviderMock
            .Setup(x => x.UnblockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Auth0Exception(Auth0FailureMessage));

        var response = await Client.PostAsync($"{UsersRoute}/{user.Id}/activate", null);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.IdentityProviderError, problem.Title);

        var unchanged = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(unchanged);
        Assert.Equal(UserAccessStatus.Inactive, unchanged.AccessStatus);

        Assert.Empty(await GetOutboxStatusChangedEventsAsync());
    }

    private void VerifyIdentityProviderNeverUnblocked() =>
        IdentityProviderMock.Verify(
            x => x.UnblockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
