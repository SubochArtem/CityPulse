using System.Net;
using System.Net.Http.Json;
using Users.Presentation.Middleware;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Users.TestData;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class GetUsersPaginationValidationTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";

    [Theory]
    [MemberData(
        nameof(GetUsersPaginationValidationTestData.InvalidPagination),
        MemberType = typeof(GetUsersPaginationValidationTestData))]
    public async Task GetUsers_InvalidPagination_ReturnsBadRequestWithValidationErrors(
        string query,
        string expectedMessage)
    {
        var response = await Client.GetAsync($"{UsersRoute}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.ValidationFailed, problem.Title);
        Assert.Contains(expectedMessage, problem.Errors);
    }

    [Theory]
    [MemberData(
        nameof(GetUsersPaginationValidationTestData.BoundaryPagination),
        MemberType = typeof(GetUsersPaginationValidationTestData))]
    public async Task GetUsers_BoundaryPagination_ReturnsOk(string query)
    {
        var response = await Client.GetAsync($"{UsersRoute}?{query}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [MemberData(
        nameof(GetUsersPaginationValidationTestData.NonNumericPagination),
        MemberType = typeof(GetUsersPaginationValidationTestData))]
    public async Task GetUsers_NonNumericPagination_ReturnsBadRequest(string query)
    {
        var response = await Client.GetAsync($"{UsersRoute}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
