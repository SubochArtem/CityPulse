using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CityPulse.Common.Api.Swagger;

internal sealed class ResponseSchemaOperationFilter(SwaggerResponseOptions options) : IOperationFilter
{
    private const string JsonContentType = "application/json";
    private const string ProblemJsonContentType = "application/problem+json";
    private const string SuccessStatusPrefix = "2";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ApplySuccessResponse(operation, context);
        ApplyProblemResponses(operation, context);
    }

    private static Type? ResolveReturnType(Type? returnType)
    {
        if (returnType is null || !returnType.IsGenericType)
        {
            return returnType;
        }

        var definition = returnType.GetGenericTypeDefinition();
        if (definition == typeof(Task<>) || definition == typeof(ValueTask<>) || definition == typeof(ActionResult<>))
        {
            return ResolveReturnType(returnType.GetGenericArguments()[0]);
        }

        return returnType;
    }

    private static void ReplaceSuccessResponse(OpenApiOperation operation, int statusCode, OpenApiSchema? schema)
    {
        foreach (var key in operation.Responses.Keys
                     .Where(key => key.StartsWith(SuccessStatusPrefix, StringComparison.Ordinal))
                     .ToList())
        {
            operation.Responses.Remove(key);
        }

        var response = new OpenApiResponse { Description = ReasonPhrases.GetReasonPhrase(statusCode) };
        if (schema is not null)
        {
            response.Content[JsonContentType] = new OpenApiMediaType { Schema = schema };
        }

        operation.Responses[statusCode.ToString(CultureInfo.InvariantCulture)] = response;
    }

    private static bool IsAuthorized(OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        return metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();
    }

    private static bool HasInput(OpenApiOperation operation) =>
        operation.RequestBody is not null || operation.Parameters.Count > 0;

    private static bool HasPathParameter(OpenApiOperation operation) =>
        operation.Parameters.Any(parameter => parameter.In == ParameterLocation.Path);

    private static bool AppliesTo(int statusCode, OpenApiOperation operation, OperationFilterContext context) =>
        statusCode switch
        {
            StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden => IsAuthorized(context),
            StatusCodes.Status400BadRequest => HasInput(operation),
            StatusCodes.Status404NotFound => HasPathParameter(operation),
            _ => true
        };

    private void ApplySuccessResponse(OpenApiOperation operation, OperationFilterContext context)
    {
        var returnType = ResolveReturnType(context.MethodInfo?.ReturnType);
        if (returnType is null)
        {
            return;
        }

        if (returnType == typeof(NoContentResult))
        {
            ReplaceSuccessResponse(operation, StatusCodes.Status204NoContent, null);

            return;
        }

        if (!options.TryUnwrap(returnType, out var valueType))
        {
            return;
        }

        if (options.IsEmpty(valueType))
        {
            ReplaceSuccessResponse(operation, StatusCodes.Status204NoContent, null);

            return;
        }

        var statusCode = HttpMethods.IsPost(context.ApiDescription.HttpMethod ?? string.Empty)
            ? StatusCodes.Status201Created
            : StatusCodes.Status200OK;

        ReplaceSuccessResponse(
            operation,
            statusCode,
            context.SchemaGenerator.GenerateSchema(valueType, context.SchemaRepository));
    }

    private void ApplyProblemResponses(OpenApiOperation operation, OperationFilterContext context)
    {
        if (options.ProblemStatusCodes.Count == 0)
        {
            return;
        }

        var schema = context.SchemaGenerator.GenerateSchema(typeof(ApiProblemDetails), context.SchemaRepository);

        foreach (var statusCode in options.ProblemStatusCodes)
        {
            var key = statusCode.ToString(CultureInfo.InvariantCulture);
            if (operation.Responses.ContainsKey(key) || !AppliesTo(statusCode, operation, context))
            {
                continue;
            }

            var response = new OpenApiResponse { Description = ReasonPhrases.GetReasonPhrase(statusCode) };
            response.Content[ProblemJsonContentType] = new OpenApiMediaType { Schema = schema };
            response.Content[JsonContentType] = new OpenApiMediaType { Schema = schema };
            operation.Responses[key] = response;
        }
    }
}
