using CityPulse.Common.Api.Auth;
using CityPulse.Common.Api.Swagger;
using CityPulse.Common.Api.Tests.Swagger.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace CityPulse.Common.Api.Tests.Swagger;

public sealed class ResponseSchemaTests
{
    private const string JsonContentType = "application/json";
    private const string ProblemJsonContentType = "application/problem+json";
    private const string DocumentName = "v1";
    private const string Title = "Test";
    private const string Domain = "tenant.example.com";
    private const string Audience = "https://test-api";
    private const string ClientId = "test-client";

    private const string ItemPath = "/items/{id}";
    private const string ItemsPath = "/items";
    private const string StatusPath = "/items/{id}/status";
    private const string PlainPath = "/items/plain/{id}";
    private const string WrappedActionResultPath = "/items/wrapped-action-result";

    private const string ItemDtoSchema = "ItemDto";
    private const string CreateItemRequestSchema = "CreateItemRequest";
    private const string ProblemSchema = "ApiProblemDetails";
    private const string WrapperMarker = "Box";
    private const string ErrorsProperty = "errors";
    private const string CodeProperty = "code";
    private const string ObjectType = "object";
    private const string ArrayType = "array";
    private const string StringType = "string";

    private static OpenApiDocument BuildDocument(Action<SwaggerResponseOptions>? configure = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers().AddApplicationPart(typeof(ItemsController).Assembly);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOptions<Auth0Settings>().Configure(settings =>
        {
            settings.Domain = Domain;
            settings.Audience = Audience;
            settings.ClientId = ClientId;
        });
        builder.Services.AddSwaggerConfiguration(Title, Title, configure);

        using var app = builder.Build();

        return app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger(DocumentName);
    }

    private static SwaggerResponseOptions ConfigureWrappers(SwaggerResponseOptions options) => options
        .UnwrapResponse(typeof(Box<>))
        .TreatAsEmpty(typeof(Nothing));

    private static OpenApiOperation Operation(OpenApiDocument document, string path, OperationType type) =>
        document.Paths[path].Operations[type];

    private static string SchemaId(OpenApiResponse response, string contentType = JsonContentType) =>
        response.Content[contentType].Schema.Reference.Id;

    [Fact]
    public void Get_WrappedResponse_DescribesDtoWithStatus200()
    {
        var document = BuildDocument(options => ConfigureWrappers(options));

        var responses = Operation(document, ItemPath, OperationType.Get).Responses;

        Assert.Equal(ItemDtoSchema, SchemaId(responses["200"]));
    }

    [Fact]
    public void Post_WrappedResponse_DescribesDtoWithStatus201InsteadOf200()
    {
        var document = BuildDocument(options => ConfigureWrappers(options));

        var responses = Operation(document, ItemsPath, OperationType.Post).Responses;

        Assert.Equal(ItemDtoSchema, SchemaId(responses["201"]));
        Assert.DoesNotContain("200", responses.Keys);
    }

    [Fact]
    public void Put_WrappedResponse_DescribesDtoWithStatus200()
    {
        var document = BuildDocument(options => ConfigureWrappers(options));

        var responses = Operation(document, ItemPath, OperationType.Put).Responses;

        Assert.Equal(ItemDtoSchema, SchemaId(responses["200"]));
    }

    [Fact]
    public void Patch_WrappedEmptyValue_DescribesStatus204WithoutBody()
    {
        var document = BuildDocument(options => ConfigureWrappers(options));

        var responses = Operation(document, StatusPath, OperationType.Patch).Responses;

        Assert.Empty(responses["204"].Content);
        Assert.DoesNotContain("200", responses.Keys);
    }

    [Fact]
    public void Get_WrappedCollection_DescribesArrayOfDto()
    {
        var document = BuildDocument(options => ConfigureWrappers(options));

        var schema = Operation(document, ItemsPath, OperationType.Get).Responses["200"].Content[JsonContentType].Schema;

        Assert.Equal(ArrayType, schema.Type);
        Assert.Equal(ItemDtoSchema, schema.Items.Reference.Id);
    }

    [Fact]
    public void Get_WrappedInActionResult_IsUnwrapped()
    {
        var document = BuildDocument(options => ConfigureWrappers(options));

        var responses = Operation(document, WrappedActionResultPath, OperationType.Get).Responses;

        Assert.Equal(ItemDtoSchema, SchemaId(responses["200"]));
    }

    [Fact]
    public void Delete_NoContentResult_DescribesStatus204EvenWithoutWrapperConfiguration()
    {
        var document = BuildDocument();

        var responses = Operation(document, ItemPath, OperationType.Delete).Responses;

        Assert.Empty(responses["204"].Content);
        Assert.DoesNotContain("200", responses.Keys);
    }

    [Fact]
    public void Get_PlainDto_IsLeftUntouched()
    {
        var document = BuildDocument(options => ConfigureWrappers(options));

        var responses = Operation(document, PlainPath, OperationType.Get).Responses;

        Assert.Equal(ItemDtoSchema, SchemaId(responses["200"]));
    }

    [Fact]
    public void Get_WrapperNotConfigured_KeepsWrapperSchema()
    {
        var document = BuildDocument();

        var responses = Operation(document, ItemPath, OperationType.Get).Responses;

        Assert.Contains(WrapperMarker, SchemaId(responses["200"]));
    }

    [Fact]
    public void Document_WrappersConfigured_RemovesUnusedWrapperSchemasAndKeepsUsedOnes()
    {
        var document = BuildDocument(options => ConfigureWrappers(options));

        var schemas = document.Components.Schemas.Keys;

        Assert.DoesNotContain(schemas, name => name.Contains(WrapperMarker));
        Assert.Contains(ItemDtoSchema, schemas);
        Assert.Contains(CreateItemRequestSchema, schemas);
    }

    [Fact]
    public void Get_ProblemsConfigured_AuthorizedOperationWithInputGetsAllProblemStatuses()
    {
        var document = BuildDocument(options => ConfigureWrappers(options).DocumentProblems(
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status409Conflict));

        var responses = Operation(document, ItemPath, OperationType.Put).Responses;

        Assert.Contains("400", responses.Keys);
        Assert.Contains("401", responses.Keys);
        Assert.Contains("403", responses.Keys);
        Assert.Contains("409", responses.Keys);
        Assert.Contains("200", responses.Keys);
    }

    [Fact]
    public void Get_ProblemsConfigured_AnonymousOperationWithoutInputGetsOnlyGeneralStatuses()
    {
        var document = BuildDocument(options => ConfigureWrappers(options).DocumentProblems(
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status409Conflict));

        var responses = Operation(document, ItemsPath, OperationType.Get).Responses;

        Assert.Equal(["200", "409"], responses.Keys.OrderBy(key => key).ToArray());
    }

    [Fact]
    public void Get_ProblemsConfigured_Status404OnlyForOperationsWithPathParameter()
    {
        var document = BuildDocument(options => ConfigureWrappers(options)
            .DocumentProblems(StatusCodes.Status404NotFound));

        Assert.Contains("404", Operation(document, ItemPath, OperationType.Get).Responses.Keys);
        Assert.DoesNotContain("404", Operation(document, ItemsPath, OperationType.Get).Responses.Keys);
    }

    [Fact]
    public void Get_ProblemsConfigured_UsesProblemSchemaForBothContentTypes()
    {
        var document = BuildDocument(options => ConfigureWrappers(options)
            .DocumentProblems(StatusCodes.Status404NotFound));

        var response = Operation(document, ItemPath, OperationType.Get).Responses["404"];

        Assert.Equal(ProblemSchema, SchemaId(response, ProblemJsonContentType));
        Assert.Equal(ProblemSchema, SchemaId(response, JsonContentType));
    }

    [Fact]
    public void Document_ProblemsConfigured_ProblemSchemaDescribesErrorsAndCode()
    {
        var document = BuildDocument(options => ConfigureWrappers(options)
            .DocumentProblems(StatusCodes.Status404NotFound));

        var schema = document.Components.Schemas[ProblemSchema];

        Assert.Equal(ObjectType, schema.Type);
        Assert.Equal(ArrayType, schema.Properties[ErrorsProperty].Type);
        Assert.Equal(StringType, schema.Properties[ErrorsProperty].Items.Type);
        Assert.Equal(StringType, schema.Properties[CodeProperty].Type);
    }

    [Fact]
    public void Put_ExplicitProducesResponseType_IsNotOverwritten()
    {
        var document = BuildDocument(options => ConfigureWrappers(options)
            .DocumentProblems(StatusCodes.Status404NotFound));

        var response = Operation(document, ItemPath, OperationType.Put).Responses["404"];

        Assert.DoesNotContain(ProblemJsonContentType, response.Content.Keys);
        Assert.Equal(StringType, response.Content.Values.First().Schema.Type);
    }

    [Fact]
    public void Document_ProblemsNotConfigured_DoesNotAddProblemSchemaOrStatuses()
    {
        var document = BuildDocument(options => ConfigureWrappers(options));

        Assert.DoesNotContain(ProblemSchema, document.Components.Schemas.Keys);
        Assert.DoesNotContain("404", Operation(document, ItemPath, OperationType.Get).Responses.Keys);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(Box<ItemDto>))]
    [InlineData(typeof(Dictionary<,>))]
    public void UnwrapResponse_NotAnOpenGenericWithOneParameter_Throws(Type type)
    {
        var options = new SwaggerResponseOptions();

        Assert.Throws<ArgumentException>(() => options.UnwrapResponse(type));
    }
}
