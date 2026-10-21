using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Users.Business.Exceptions;
using Users.Presentation.Middleware;
using Users.Tests.Infrastructure;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class DeleteUserTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";
    private const string Auth0FailureMessage = "Auth0 is down";

    [Fact]
    public async Task DeleteUser_UserExists_DeletesUserFromIdentityProviderAndDatabase()
    {
        var user = await SeedUserAsync();
        var otherUser = await SeedUserAsync();

        var response = await Client.DeleteAsync($"{UsersRoute}/{user.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Null(await GetUserFromDbAsync(user.Id));
        Assert.NotNull(await GetUserFromDbAsync(otherUser.Id));

        IdentityProviderMock.Verify(
            x => x.DeleteUserAsync(user.IdentityId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUser_UserDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var response = await Client.DeleteAsync($"{UsersRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.UserNotFound, problem.Title);

        IdentityProviderMock.Verify(
            x => x.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUser_IdentityProviderFails_ReturnsBadGatewayAndKeepsUser()
    {
        var user = await SeedUserAsync();
        IdentityProviderMock
            .Setup(x => x.DeleteUserAsync(user.IdentityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Auth0Exception(Auth0FailureMessage));

        var response = await Client.DeleteAsync($"{UsersRoute}/{user.Id}");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.IdentityProviderError, problem.Title);

        Assert.NotNull(await GetUserFromDbAsync(user.Id));
    }
}
