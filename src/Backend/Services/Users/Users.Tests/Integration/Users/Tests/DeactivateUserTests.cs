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

public sealed class DeactivateUserTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";
    private const string Auth0FailureMessage = "Auth0 is down";

    [Theory]
    [MemberData(
        nameof(DeactivateUserTestData.StatusesThatCanBeDeactivated),
        MemberType = typeof(DeactivateUserTestData))]
    public async Task DeactivateUser_UserIsNotInactive_BlocksUserSavesStatusAndPublishesEvent(UserAccessStatus status)
    {
        var user = await SeedUserAsync(status);

        var response = await Client.PostAsync($"{UsersRoute}/{user.Id}/deactivate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal(UserAccessStatus.Inactive, updated.AccessStatus);
        Assert.NotNull(updated.UpdatedAt);
        Assert.True(updated.UpdatedAt > user.UpdatedAt);

        IdentityProviderMock.Verify(
            x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);

        var published = Assert.Single(await GetOutboxStatusChangedEventsAsync());
        Assert.Equal(user.Id, published.UserId);
        Assert.Equal(UserLifecycleStatus.Inactive, published.UserLifecycleStatus);
    }

    [Fact]
    public async Task DeactivateUser_UserIsAlreadyInactive_ReturnsOkAndDoesNothing()
    {
        var user = await SeedUserAsync(UserAccessStatus.Inactive);

        var response = await Client.PostAsync($"{UsersRoute}/{user.Id}/deactivate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var unchanged = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(unchanged);
        Assert.Equal(UserAccessStatus.Inactive, unchanged.AccessStatus);

        VerifyIdentityProviderNeverBlocked();
        Assert.Empty(await GetOutboxStatusChangedEventsAsync());
    }

    [Fact]
    public async Task DeactivateUser_UserDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var response = await Client.PostAsync($"{UsersRoute}/{Guid.NewGuid()}/deactivate", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.UserNotFound, problem.Title);

        VerifyIdentityProviderNeverBlocked();
        Assert.Empty(await GetOutboxStatusChangedEventsAsync());
    }

    [Fact]
    public async Task DeactivateUser_IdentityProviderFails_ReturnsBadGatewayAndKeepsUserActive()
    {
        var user = await SeedUserAsync(UserAccessStatus.Active);
        IdentityProviderMock
            .Setup(x => x.BlockUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Auth0Exception(Auth0FailureMessage));

        var response = await Client.PostAsync($"{UsersRoute}/{user.Id}/deactivate", null);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.IdentityProviderError, problem.Title);

        var unchanged = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(unchanged);
        Assert.Equal(UserAccessStatus.Active, unchanged.AccessStatus);

        Assert.Empty(await GetOutboxStatusChangedEventsAsync());
    }

    private void VerifyIdentityProviderNeverBlocked() =>
        IdentityProviderMock.Verify(
            x => x.BlockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
