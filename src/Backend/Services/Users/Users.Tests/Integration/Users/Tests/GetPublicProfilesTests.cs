using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Presentation.Middleware;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Users.TestData;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class GetPublicProfilesTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string ProfilesRoute = "api/v1/Users/profiles";
    private static readonly string[] ExpectedProperties = ["id", "nickname"];

    private static string Route(IEnumerable<Guid> ids) =>
        $"{ProfilesRoute}?{GetPublicProfilesTestData.BuildQuery(ids)}";

    [Fact]
    public async Task GetPublicProfiles_AnonymousRequest_ReturnsOkWithNicknames()
    {
        var first = await SeedUserAsync();
        var second = await SeedUserAsync();
        using var client = CreateAnonymousClient();

        var response = await client.GetAsync(Route([first.Id, second.Id]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profiles = await response.Content.ReadFromJsonAsync<List<PublicUserProfileDto>>();
        Assert.NotNull(profiles);
        Assert.Equal(2, profiles.Count);
        Assert.Contains(profiles, x => x.Id == first.Id && x.Nickname == first.Nickname);
        Assert.Contains(profiles, x => x.Id == second.Id && x.Nickname == second.Nickname);
    }

    [Fact]
    public async Task GetPublicProfiles_UserExists_ExposesOnlyIdAndNickname()
    {
        var user = await SeedUserAsync();
        using var client = CreateAnonymousClient();

        var response = await client.GetAsync(Route([user.Id]));

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = Assert.Single(document.RootElement.EnumerateArray());
        var properties = item.EnumerateObject().Select(p => p.Name.ToLowerInvariant()).Order().ToArray();
        Assert.Equal(ExpectedProperties, properties);
    }

    [Fact]
    public async Task GetPublicProfiles_SomeIdsDoNotExist_SkipsUnknownIds()
    {
        var user = await SeedUserAsync();

        var response = await Client.GetAsync(Route([user.Id, Guid.NewGuid()]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profiles = await response.Content.ReadFromJsonAsync<List<PublicUserProfileDto>>();
        Assert.NotNull(profiles);
        Assert.Equal(user.Id, Assert.Single(profiles).Id);
    }

    [Fact]
    public async Task GetPublicProfiles_NoIdsExist_ReturnsEmptyList()
    {
        var response = await Client.GetAsync(Route([Guid.NewGuid()]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profiles = await response.Content.ReadFromJsonAsync<List<PublicUserProfileDto>>();
        Assert.NotNull(profiles);
        Assert.Empty(profiles);
    }

    [Fact]
    public async Task GetPublicProfiles_DuplicateIds_ReturnsUserOnce()
    {
        var user = await SeedUserAsync();

        var response = await Client.GetAsync(Route([user.Id, user.Id, user.Id]));

        var profiles = await response.Content.ReadFromJsonAsync<List<PublicUserProfileDto>>();
        Assert.NotNull(profiles);
        Assert.Equal(user.Id, Assert.Single(profiles).Id);
    }

    [Fact]
    public async Task GetPublicProfiles_MaximumNumberOfIds_ReturnsOk()
    {
        var ids = Enumerable.Range(0, ValidationConstants.PublicProfiles.MaxIds).Select(_ => Guid.NewGuid());

        var response = await Client.GetAsync(Route(ids));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetPublicProfilesTestData.InvalidQueries), MemberType = typeof(GetPublicProfilesTestData))]
    public async Task GetPublicProfiles_InvalidIdsCount_ReturnsBadRequestWithValidationErrors(
        string query,
        string expectedMessage)
    {
        var response = await Client.GetAsync($"{ProfilesRoute}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.ValidationFailed, problem.Title);
        Assert.Contains(expectedMessage, problem.Errors);
    }

    [Theory]
    [MemberData(nameof(GetPublicProfilesTestData.NonGuidQueries), MemberType = typeof(GetPublicProfilesTestData))]
    public async Task GetPublicProfiles_NonGuidId_ReturnsBadRequest(string query)
    {
        var response = await Client.GetAsync($"{ProfilesRoute}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
