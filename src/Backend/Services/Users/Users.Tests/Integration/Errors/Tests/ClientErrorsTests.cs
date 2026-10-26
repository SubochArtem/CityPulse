using System.Net;
using System.Text;
using System.Text.Json;
using Users.Tests.Infrastructure;
using Xunit;

namespace Users.Tests.Integration.Errors.Tests;

public sealed class ClientErrorsTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";
    private const string UnknownRoute = "api/v1/does-not-exist";
    private const string JsonContentType = "application/json";
    private const string MalformedJson = "{bad";
    private const string TitleProperty = "title";
    private const string StatusProperty = "status";
    private const string DetailProperty = "detail";
    private const string ErrorsProperty = "errors";
    private const string InstanceProperty = "instance";
    private const string NotFoundTitle = "NotFound";
    private const string UnauthorizedTitle = "Unauthorized";
    private const string ForbiddenTitle = "Forbidden";
    private const string ValidationTitle = "Validation";
    private const string InvalidBodyMessage = "The request body is invalid.";

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

    private static void AssertProblem(
        JsonElement problem,
        HttpStatusCode expectedStatus,
        string expectedTitle,
        string expectedPath)
    {
        Assert.Equal(expectedTitle, problem.GetProperty(TitleProperty).GetString());
        Assert.Equal((int)expectedStatus, problem.GetProperty(StatusProperty).GetInt32());
        Assert.Equal($"/{expectedPath}", problem.GetProperty(InstanceProperty).GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty(DetailProperty).GetString()));
        Assert.Equal(JsonValueKind.Array, problem.GetProperty(ErrorsProperty).ValueKind);
        Assert.NotEmpty(problem.GetProperty(ErrorsProperty).EnumerateArray());
    }

    [Fact]
    public async Task UnknownRoute_ReturnsNotFoundProblem()
    {
        var response = await CreateClient([]).GetAsync(UnknownRoute);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        AssertProblem(await ReadProblemAsync(response), HttpStatusCode.NotFound, NotFoundTitle, UnknownRoute);
    }

    [Fact]
    public async Task ProtectedEndpoint_AnonymousUser_ReturnsUnauthorizedProblem()
    {
        var response = await CreateAnonymousClient().GetAsync(UsersRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        AssertProblem(await ReadProblemAsync(response), HttpStatusCode.Unauthorized, UnauthorizedTitle, UsersRoute);
    }

    [Fact]
    public async Task ProtectedEndpoint_UserWithoutPermissions_ReturnsForbiddenProblem()
    {
        var response = await CreateClient([]).GetAsync(UsersRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        AssertProblem(await ReadProblemAsync(response), HttpStatusCode.Forbidden, ForbiddenTitle, UsersRoute);
    }

    [Fact]
    public async Task MalformedJsonBody_ReturnsValidationProblemWithoutParserDetails()
    {
        var user = await SeedUserAsync();
        using var body = new StringContent(MalformedJson, Encoding.UTF8, JsonContentType);
        var route = $"{UsersRoute}/{user.Id}";

        var response = await Client.PatchAsync(route, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        AssertProblem(problem, HttpStatusCode.BadRequest, ValidationTitle, route);
        Assert.Contains(
            InvalidBodyMessage,
            problem.GetProperty(ErrorsProperty).EnumerateArray().Select(e => e.GetString()));
    }
}
