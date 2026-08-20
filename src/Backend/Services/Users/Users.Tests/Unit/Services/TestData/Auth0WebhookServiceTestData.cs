using Xunit;

namespace Users.Tests.Unit.Services.TestData;

public static class Auth0WebhookServiceTestData
{
    public static TheoryData<string?> EmptyOrNullSignatures => new()
    {
        null,
        "",
        "   "
    };

    public static TheoryData<string> EmptySecrets => new()
    {
        "",
        "   "
    };
}
