using Users.Business.Configurations;
using Users.Tests.Infrastructure;
using Xunit;

namespace Users.Tests.Integration.Webhook.TestData;

public static class Auth0WebhookRejectionTestData
{
    public const string OtherSecret = "another-webhook-secret";
    public const string OtherEventBody = """{"event":"user.created","iss":"https://x/","user":{"id":"auth0|other"}}""";

    public static TheoryData<string> MalformedSignatures => new()
    {
        "",
        "   ",
        IdentityProviderConstants.WebhookSignaturePrefix,
        $"{IdentityProviderConstants.WebhookSignaturePrefix}deadbeef",
        "not-a-signature"
    };

    public static TheoryData<string> PayloadsWithoutUserId => new()
    {
        "{}",
        "null",
        Auth0WebhookRequestBuilder.BuildPayload(userId: null)
    };

    public static TheoryData<string> WrongIssuers => new()
    {
        "https://other-tenant.example.com/",
        "https://test-tenant.example.com",
        "http://test-tenant.example.com/",
        ""
    };

    public static TheoryData<string> UnsupportedEvents => new()
    {
        "user.deleted",
        "user.updated",
        "USER.CREATED",
        ""
    };
}
