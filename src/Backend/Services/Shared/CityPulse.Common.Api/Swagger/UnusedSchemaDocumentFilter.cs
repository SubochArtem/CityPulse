using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CityPulse.Common.Api.Swagger;

internal sealed class UnusedSchemaDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var schemas = swaggerDoc.Components?.Schemas;
        if (schemas is null || schemas.Count == 0)
        {
            return;
        }

        var reachable = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<OpenApiSchema>(ReferenceEqualityComparer.Instance);
        var pending = new Stack<OpenApiSchema>(CollectOperationSchemas(swaggerDoc));

        while (pending.Count > 0)
        {
            var schema = pending.Pop();

            if (schema.Reference is { Type: ReferenceType.Schema, Id: { } id })
            {
                if (!reachable.Add(id) || !schemas.TryGetValue(id, out var target))
                {
                    continue;
                }

                pending.Push(target);

                continue;
            }

            if (!visited.Add(schema))
            {
                continue;
            }

            foreach (var child in Children(schema))
            {
                pending.Push(child);
            }
        }

        foreach (var key in schemas.Keys.Where(key => !reachable.Contains(key)).ToList())
        {
            schemas.Remove(key);
        }
    }

    private static IEnumerable<OpenApiSchema> Children(OpenApiSchema schema)
    {
        foreach (var property in schema.Properties.Values)
        {
            yield return property;
        }

        if (schema.Items is not null)
        {
            yield return schema.Items;
        }

        if (schema.AdditionalProperties is not null)
        {
            yield return schema.AdditionalProperties;
        }

        if (schema.Not is not null)
        {
            yield return schema.Not;
        }

        foreach (var child in schema.AllOf.Concat(schema.OneOf).Concat(schema.AnyOf))
        {
            yield return child;
        }
    }

    private static IEnumerable<OpenApiSchema> CollectOperationSchemas(OpenApiDocument document)
    {
        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations.Values))
        {
            foreach (var parameter in operation.Parameters.Where(parameter => parameter.Schema is not null))
            {
                yield return parameter.Schema;
            }

            var bodies = operation.RequestBody?.Content.Values ?? Enumerable.Empty<OpenApiMediaType>();
            var mediaTypes = bodies.Concat(operation.Responses.Values.SelectMany(response => response.Content.Values));
            foreach (var media in mediaTypes.Where(media => media.Schema is not null))
            {
                yield return media.Schema;
            }
        }
    }
}
