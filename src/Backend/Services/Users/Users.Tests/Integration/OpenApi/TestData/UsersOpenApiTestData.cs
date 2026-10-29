using Xunit;

namespace Users.Tests.Integration.OpenApi.TestData;

public static class UsersOpenApiTestData
{
    public static TheoryData<string, string> NoContentOperations => new()
    {
        { "/api/v1/Users/{id}/activate", "post" },
        { "/api/v1/Users/{id}/deactivate", "post" },
        { "/api/v1/Users/{id}", "delete" }
    };
}
