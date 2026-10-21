using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Users.Business.Configurations;
using Users.Presentation.Middleware;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Webhook.TestData;
using Xunit;

namespace Users.Tests.Integration.Webhook.Tests;

public sealed class Auth0WebhookRejectionTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    [Fact]
    public async Task HandleWebhook_SignatureHeaderIsMissing_ReturnsUnauthorized()
    {
        var response = await PostWebhookAsync(Auth0WebhookRequestBuilder.BuildPayload(), signature: null);

        await AssertRejectedAsync(response, HttpStatusCode.Unauthorized, MiddlewareExceptionMessages.Titles.Unauthorized);
    }

    [Theory]
    [MemberData(
        nameof(Auth0WebhookRejectionTestData.MalformedSignatures),
        MemberType = typeof(Auth0WebhookRejectionTestData))]
    public async Task HandleWebhook_SignatureIsMalformed_ReturnsUnauthorized(string signature)
    {
        var response = await PostWebhookAsync(Auth0WebhookRequestBuilder.BuildPayload(), signature);

        await AssertRejectedAsync(response, HttpStatusCode.Unauthorized, MiddlewareExceptionMessages.Titles.Unauthorized);
    }

    [Fact]
    public async Task HandleWebhook_SignedWithWrongSecret_ReturnsUnauthorized()
    {
        var body = Auth0WebhookRequestBuilder.BuildPayload();

        var response = await PostWebhookAsync(
            body,
            Auth0WebhookRequestBuilder.Sign(body, Auth0WebhookRejectionTestData.OtherSecret));

        await AssertRejectedAsync(response, HttpStatusCode.Unauthorized, MiddlewareExceptionMessages.Titles.Unauthorized);
    }

    [Fact]
    public async Task HandleWebhook_SignatureBelongsToDifferentBody_ReturnsUnauthorized()
    {
        var body = Auth0WebhookRequestBuilder.BuildPayload();

        var response = await PostWebhookAsync(
            body,
            Auth0WebhookRequestBuilder.Sign(Auth0WebhookRejectionTestData.OtherEventBody));

        await AssertRejectedAsync(response, HttpStatusCode.Unauthorized, MiddlewareExceptionMessages.Titles.Unauthorized);
    }

    [Fact]
    public async Task HandleWebhook_SignatureWithoutPrefix_ReturnsUnauthorized()
    {
        var body = Auth0WebhookRequestBuilder.BuildPayload();
        var signatureWithoutPrefix = Auth0WebhookRequestBuilder.Sign(body)
            [IdentityProviderConstants.WebhookSignaturePrefix.Length..];

        var response = await PostWebhookAsync(body, signatureWithoutPrefix);

        await AssertRejectedAsync(response, HttpStatusCode.Unauthorized, MiddlewareExceptionMessages.Titles.Unauthorized);
    }

    [Fact]
    public async Task HandleWebhook_SignatureHexIsUpperCase_ReturnsUnauthorized()
    {
        var body = Auth0WebhookRequestBuilder.BuildPayload();
        var signature = Auth0WebhookRequestBuilder.Sign(body);
        var upperCaseHex = signature[IdentityProviderConstants.WebhookSignaturePrefix.Length..].ToUpperInvariant();

        var response = await PostWebhookAsync(
            body,
            $"{IdentityProviderConstants.WebhookSignaturePrefix}{upperCaseHex}");

        await AssertRejectedAsync(response, HttpStatusCode.Unauthorized, MiddlewareExceptionMessages.Titles.Unauthorized);
    }

    [Theory]
    [MemberData(
        nameof(Auth0WebhookRejectionTestData.PayloadsWithoutUserId),
        MemberType = typeof(Auth0WebhookRejectionTestData))]
    public async Task HandleWebhook_PayloadHasNoUserId_ReturnsBadRequest(string body)
    {
        var response = await PostWebhookAsync(body, Auth0WebhookRequestBuilder.Sign(body));

        await AssertRejectedAsync(response, HttpStatusCode.BadRequest, MiddlewareExceptionMessages.Titles.BadRequest);
    }

    [Theory]
    [MemberData(
        nameof(Auth0WebhookRejectionTestData.WrongIssuers),
        MemberType = typeof(Auth0WebhookRejectionTestData))]
    public async Task HandleWebhook_IssuerDoesNotMatchDomain_ReturnsBadRequest(string issuer)
    {
        var body = Auth0WebhookRequestBuilder.BuildPayload(iss: issuer);

        var response = await PostWebhookAsync(body, Auth0WebhookRequestBuilder.Sign(body));

        await AssertRejectedAsync(response, HttpStatusCode.BadRequest, MiddlewareExceptionMessages.Titles.BadRequest);
    }

    [Theory]
    [MemberData(
        nameof(Auth0WebhookRejectionTestData.UnsupportedEvents),
        MemberType = typeof(Auth0WebhookRejectionTestData))]
    public async Task HandleWebhook_EventIsNotUserCreated_IgnoresEventAndDoesNotCreateUser(string eventName)
    {
        var body = Auth0WebhookRequestBuilder.BuildPayload(eventName: eventName);

        var response = await PostWebhookAsync(body, Auth0WebhookRequestBuilder.Sign(body));

        await AssertRejectedAsync(response, HttpStatusCode.OK, MiddlewareExceptionMessages.Titles.WebhookEventIgnored);
    }

    private async Task AssertRejectedAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedTitle)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(expectedTitle, problem.Title);

        Assert.Equal(0, await CountUsersInDbAsync());
    }
}
