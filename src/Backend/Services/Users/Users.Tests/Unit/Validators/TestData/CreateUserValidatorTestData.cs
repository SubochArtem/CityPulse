using Xunit;

namespace Users.Tests.Unit.Validators.TestData;

public static class CreateUserValidatorTestData
{
    public static TheoryData<string?> EmptyIdentityIds => new()
    {
        null,
        "",
        "   "
    };

    public static TheoryData<string> InvalidIdentityIds => new()
    {
        "auth0",
        "auth0|",
        "|abc123",
        "auth0 abc123",
        "auth0|abc 123",
        "auth0|abc|123"
    };

    public static TheoryData<string> ValidIdentityIds => new()
    {
        "auth0|abc123",
        "google-oauth2|1234567890",
        "windowslive|abc.def-123_456"
    };

    public static TheoryData<string?> EmptyNicknames => new()
    {
        null,
        ""
    };
}
