using System.Net;
using System.Net.Http.Json;
using Users.Business.DTOs;
using Users.Presentation.Middleware;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Users.TestData;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class UpdateUserValidationTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";

    [Theory]
    [MemberData(
        nameof(UpdateUserValidationTestData.InvalidNicknames),
        MemberType = typeof(UpdateUserValidationTestData))]
    public async Task UpdateUser_InvalidNickname_ReturnsBadRequestWithValidationErrorsAndDoesNotChangeUser(
        string nickname,
        string expectedMessage)
    {
        var user = await SeedUserAsync();

        var response = await Client.PatchAsJsonAsync(
            $"{UsersRoute}/{user.Id}",
            new UpdateUserProfileDto { Nickname = nickname });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal(MiddlewareExceptionMessages.Titles.ValidationFailed, problem.Title);
        Assert.True(problem.Errors.TryGetValue(UpdateUserValidationTestData.NicknameKey, out var messages));
        Assert.Contains(expectedMessage, messages);

        var unchanged = await GetUserFromDbAsync(user.Id);
        Assert.NotNull(unchanged);
        Assert.Equal(user.Nickname, unchanged.Nickname);

        VerifyIdentityProviderNeverUpdated();
    }
}
