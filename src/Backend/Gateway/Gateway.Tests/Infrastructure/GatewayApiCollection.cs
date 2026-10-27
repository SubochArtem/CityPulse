using Xunit;

namespace Gateway.Tests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class GatewayApiCollection : ICollectionFixture<GatewayWebAppFactory>
{
    public const string Name = "GatewayApi";
}
