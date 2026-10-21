using Xunit;

namespace Users.Tests.Integration.Webhook.TestData;

public static class Auth0WebhookCreateUserTestData
{
    public const string EmailLocalPart = "someone";
    public const string Email = "someone@example.com";
    public const string ProvidedNickname = "provided_nickname";
    public const string GeneratedNicknamePrefix = "User_";
    public const int GeneratedNicknameLength = 13;

    public static TheoryData<string?, string?, string> NicknameSources => new()
    {
        { ProvidedNickname, Email, ProvidedNickname },
        { ProvidedNickname, null, ProvidedNickname },
        { null, Email, EmailLocalPart },
        { "", Email, EmailLocalPart },
        { "   ", Email, EmailLocalPart }
    };

    public static TheoryData<string?, string?> GeneratedNicknameSources => new()
    {
        { null, null },
        { "", "" },
        { null, "no-at-sign" }
    };
}
