using System.Net.Http.Json;
using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Gateway.Tests.Infrastructure;

[Collection(GatewayApiCollection.Name)]
[Trait(TestConstants.Traits.Category, TestConstants.Traits.Integration)]
public abstract class GatewayApiTestBase(GatewayWebAppFactory factory)
{
    protected GatewayWebAppFactory Factory { get; } = factory;

    protected HttpClient Client { get; } = factory.CreateClient();

    protected HttpClient CreateClient(IReadOnlyDictionary<string, string?> settings) =>
        Factory.WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(settings)))
            .CreateClient();

    protected static async Task<BackendEcho> ReadEchoAsync(HttpResponseMessage response)
    {
        var echo = await response.Content.ReadFromJsonAsync<BackendEcho>();

        return echo ?? throw new InvalidOperationException("Backend echo was empty.");
    }

    protected static HttpRequestMessage CreateRequest(HttpMethod method, string path, string? origin = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (origin is not null)
        {
            request.Headers.Add(TestConstants.Headers.Origin, origin);
        }

        return request;
    }

    protected static string? GetAllowOrigin(HttpResponseMessage response) =>
        response.Headers.TryGetValues(TestConstants.Headers.AccessControlAllowOrigin, out var values)
            ? values.Single()
            : null;
}
