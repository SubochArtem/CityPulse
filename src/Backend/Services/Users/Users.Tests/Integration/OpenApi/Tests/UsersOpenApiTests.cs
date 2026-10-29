using System.Text.Json;
using Users.Business.Configurations;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.OpenApi.TestData;
using Xunit;

namespace Users.Tests.Integration.OpenApi.Tests;

public sealed class UsersOpenApiTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string DocumentPath = "/swagger/v1/swagger.json";
    private const string WebhookPath = "/api/v1/Auth0Webhook";
    private const string ProblemSchema = "ApiProblemDetails";
    private const string ProblemContentType = "application/problem+json";
    private const string JsonContentType = "application/json";
    private const string HeaderLocation = "header";

    private async Task<JsonDocument> LoadDocumentAsync()
    {
        var response = await Client.GetAsync(DocumentPath);
        response.EnsureSuccessStatusCode();

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static JsonElement Operation(JsonDocument document, string path, string method) =>
        document.RootElement.GetProperty("paths").GetProperty(path).GetProperty(method);

    [Fact]
    public async Task Webhook_SignatureHeader_IsDocumentedAsOptionalHeaderParameter()
    {
        using var document = await LoadDocumentAsync();

        var parameter = Operation(document, WebhookPath, "post")
            .GetProperty("parameters")
            .EnumerateArray()
            .Single(p => p.GetProperty("name").GetString() == IdentityProviderConstants.WebhookSignatureHeader);

        Assert.Equal(HeaderLocation, parameter.GetProperty("in").GetString());
        Assert.False(parameter.TryGetProperty("required", out var required) && required.GetBoolean());
    }

    [Fact]
    public async Task Webhook_Responses_AreDocumentedWithUserAndProblemSchemas()
    {
        using var document = await LoadDocumentAsync();

        var responses = Operation(document, WebhookPath, "post").GetProperty("responses");

        Assert.True(responses.TryGetProperty("200", out _));
        foreach (var status in new[] { "400", "401" })
        {
            var schema = responses.GetProperty(status).GetProperty("content").GetProperty(ProblemContentType)
                .GetProperty("schema").GetProperty("$ref").GetString();
            Assert.EndsWith(ProblemSchema, schema);
        }
    }

    [Theory]
    [MemberData(nameof(UsersOpenApiTestData.NoContentOperations), MemberType = typeof(UsersOpenApiTestData))]
    public async Task NoContentOperation_IsDocumentedAs204Only(string path, string method)
    {
        using var document = await LoadDocumentAsync();

        var responses = Operation(document, path, method).GetProperty("responses");

        Assert.True(responses.TryGetProperty("204", out _));
        Assert.False(responses.TryGetProperty("200", out _));
    }

    [Fact]
    public async Task Document_ProblemSchema_DescribesErrorsAsStringArray()
    {
        using var document = await LoadDocumentAsync();

        var errors = document.RootElement.GetProperty("components").GetProperty("schemas")
            .GetProperty(ProblemSchema).GetProperty("properties").GetProperty("errors");

        Assert.Equal("array", errors.GetProperty("type").GetString());
        Assert.Equal("string", errors.GetProperty("items").GetProperty("type").GetString());
    }

    [Fact]
    public async Task Document_AuthorizedOperation_DocumentsUnauthorizedAndForbiddenProblems()
    {
        using var document = await LoadDocumentAsync();

        var responses = Operation(document, "/api/v1/Users", "get").GetProperty("responses");

        foreach (var status in new[] { "401", "403" })
        {
            Assert.True(responses.GetProperty(status).GetProperty("content").TryGetProperty(JsonContentType, out _));
        }
    }
}
