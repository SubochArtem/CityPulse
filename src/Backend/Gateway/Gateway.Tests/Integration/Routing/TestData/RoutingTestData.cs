using Gateway.Tests.Infrastructure.Constants;
using Xunit;

namespace Gateway.Tests.Integration.Routing.TestData;

public static class RoutingTestData
{
    private const string Id = "3f2c1b7e-5d4a-4e8f-9a6b-1c0d2e3f4a5b";

    public static TheoryData<string, string, string> FeedbackRoutes => new()
    {
        { "GET", "/api/v1/feedback", TestConstants.Services.Feedback },
        { "PUT", $"/api/v1/feedback/{Id}", TestConstants.Services.Feedback },
        { "DELETE", $"/api/v1/feedback/{Id}", TestConstants.Services.Feedback },
        { "POST", $"/api/v1/ideas/{Id}/feedback", TestConstants.Services.Feedback },
        { "PUT", $"/api/v1/admin/feedback/{Id}", TestConstants.Services.Feedback },
        { "DELETE", $"/api/v1/admin/feedback/{Id}", TestConstants.Services.Feedback },
        { "POST", $"/api/v1/admin/ideas/{Id}/feedback", TestConstants.Services.Feedback }
    };

    public static TheoryData<string, string, string> UsersRoutes => new()
    {
        { "GET", "/api/v1/users", TestConstants.Services.Users },
        { "GET", $"/api/v1/users/{Id}", TestConstants.Services.Users },
        { "POST", $"/api/v1/users/{Id}/activate", TestConstants.Services.Users },
        { "POST", $"/api/v1/users/{Id}/deactivate", TestConstants.Services.Users },
        { "PATCH", $"/api/v1/users/{Id}", TestConstants.Services.Users },
        { "DELETE", $"/api/v1/users/{Id}", TestConstants.Services.Users },
        { "POST", "/api/v1/auth0webhook", TestConstants.Services.Users }
    };

    public static TheoryData<string, string, string> PollsRoutes => new()
    {
        { "GET", "/api/v1/cities", TestConstants.Services.Polls },
        { "GET", "/api/v1/polls", TestConstants.Services.Polls },
        { "GET", "/api/v1/ideas", TestConstants.Services.Polls },
        { "GET", $"/api/v1/ideas/{Id}", TestConstants.Services.Polls },
        { "GET", $"/api/v1/ideas/{Id}/poll", TestConstants.Services.Polls },
        { "GET", "/api/v1/admin/cities", TestConstants.Services.Polls },
        { "GET", "/api/v1/admin/polls", TestConstants.Services.Polls },
        { "GET", "/api/v1/admin/ideas", TestConstants.Services.Polls },
        { "GET", $"/api/v1/admin/ideas/{Id}", TestConstants.Services.Polls }
    };

    public static TheoryData<string> UnroutedPaths => new()
    {
        "/",
        "/foo",
        "/api/v2/polls",
        "/api"
    };
}
