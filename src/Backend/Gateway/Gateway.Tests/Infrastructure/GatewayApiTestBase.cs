using System.Net.Http.Json;
using Gateway.Tests.Infrastructure.Constants;
using Xunit;

namespace Gateway.Tests.Infrastructure;

[Collection(GatewayApiCollection.Name)]
[Trait(TestConstants.Traits.Category, TestConstants.Traits.Integration)]
public abstract class GatewayApiTestBase(GatewayWebAppFactory factory)
{
    protected GatewayWebAppFactory Factory { get; } = factory;

    protected HttpClient Client { get; } = factory.CreateClient();

    protected static async Task<BackendEcho> ReadEchoAsync(HttpResponseMessage response)
    {
        var echo = await response.Content.ReadFromJsonAsync<BackendEcho>();

        return echo ?? throw new InvalidOperationException("Backend echo was empty.");
    }
}
