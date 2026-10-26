using System.Net;
using System.Text.Json;
using Polls.Domain.Authorization;
using Polls.Tests.Infrastructure;
using Polls.Tests.Integration.Handlers.Cities;
using Xunit;

namespace Polls.Tests.Integration.Errors.Tests;

public sealed class ClientErrorsTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string UnknownRoute = "api/v1/does-not-exist";
    private const string TitleProperty = "title";
    private const string StatusProperty = "status";
    private const string DetailProperty = "detail";
    private const string ErrorsProperty = "errors";
    private const string InstanceProperty = "instance";
    private const string NotFoundTitle = "NotFound";
    private const string UnauthorizedTitle = "Unauthorized";
    private const string ForbiddenTitle = "Forbidden";
    private const string ValidationTitle = "Validation";
    private const string TitleRequiredMessage = "The Title field is required.";

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
        var response = await CreateAnonymousClient().GetAsync(CityIntegrationHelpers.AdminCitiesRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        AssertProblem(
            await ReadProblemAsync(response),
            HttpStatusCode.Unauthorized,
            UnauthorizedTitle,
            CityIntegrationHelpers.AdminCitiesRoute);
    }

    [Fact]
    public async Task ProtectedEndpoint_UserWithoutPermissions_ReturnsForbiddenProblem()
    {
        var response = await CreateClient([]).GetAsync(CityIntegrationHelpers.AdminCitiesRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        AssertProblem(
            await ReadProblemAsync(response),
            HttpStatusCode.Forbidden,
            ForbiddenTitle,
            CityIntegrationHelpers.AdminCitiesRoute);
    }

    [Fact]
    public async Task ModelBindingFailure_ReturnsValidationProblemWithErrorsArray()
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent("1"), "Coordinates.Latitude" },
            { new StringContent("2"), "Coordinates.Longitude" }
        };

        var response = await CreateClient([Permissions.Cities.CreateAny])
            .PostAsync(CityIntegrationHelpers.AdminCitiesRoute, form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        AssertProblem(problem, HttpStatusCode.BadRequest, ValidationTitle, CityIntegrationHelpers.AdminCitiesRoute);
        Assert.Contains(
            TitleRequiredMessage,
            problem.GetProperty(ErrorsProperty).EnumerateArray().Select(e => e.GetString()));
    }
}
