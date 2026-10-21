using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Users.Business.DTOs;
using Users.Presentation.Middleware;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Users.TestData;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class GetUserByIdTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";

    [Fact]
    public async Task GetUserById_UserExists_ReturnsOkWithUser()
    {
        var user = await SeedUserAsync();

        var response = await Client.GetAsync($"{UsersRoute}/{user.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<GetUserDto>();
        Assert.NotNull(dto);
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.Nickname, dto.Nickname);
        Assert.Equal(user.CityId, dto.CityId);
        Assert.Equal(user.IdentityId, dto.IdentityId);
        Assert.Equal((int)user.AccessStatus, dto.AccessStatus);
    }

    [Fact]
    public async Task GetUserById_UserDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var response = await Client.GetAsync($"{UsersRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.NotFound, problem.Status);
        Assert.Equal(MiddlewareExceptionMessages.Titles.UserNotFound, problem.Title);
    }

    [Theory]
    [MemberData(nameof(GetUserByIdTestData.InvalidGuids), MemberType = typeof(GetUserByIdTestData))]
    public async Task GetUserById_InvalidGuid_ReturnsNotFound(string id)
    {
        var response = await Client.GetAsync($"{UsersRoute}/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
