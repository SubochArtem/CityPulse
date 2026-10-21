using Xunit;

namespace Users.Tests.Unit.Helpers.TestData;

public static class UriHelperTestData
{
    public static TheoryData<string, string?, string> ValidUriCases => new()
    {
        { "example.com", null, "https://example.com/" },
        { "example.com", "api/v1", "https://example.com/api/v1" },
        { "example.com", "/api/v1", "https://example.com/api/v1" },
        { "tenant.eu.auth0.com", "oauth/token", "https://tenant.eu.auth0.com/oauth/token" }
    };

    public static TheoryData<string> InvalidHosts => new()
    {
        "https://example.com",
        "http://example.com",
        "example.com:8080"   
    };
}
