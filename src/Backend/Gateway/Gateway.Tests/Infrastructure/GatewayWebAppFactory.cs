using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Gateway.Tests.Infrastructure;

public sealed class GatewayWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private StubBackend _users = null!;
    private StubBackend _polls = null!;
    private StubBackend _feedback = null!;

    public async Task InitializeAsync()
    {
        _users = await StubBackend.StartAsync(TestConstants.Services.Users);
        _polls = await StubBackend.StartAsync(TestConstants.Services.Polls);
        _feedback = await StubBackend.StartAsync(TestConstants.Services.Feedback);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _users.DisposeAsync();
        await _polls.DisposeAsync();
        await _feedback.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(TestConstants.Environment);
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [TestConstants.ConfigurationKeys.ClusterAddress(TestConstants.Services.Users)] = _users.Address,
                [TestConstants.ConfigurationKeys.ClusterAddress(TestConstants.Services.Polls)] = _polls.Address,
                [TestConstants.ConfigurationKeys.ClusterAddress(TestConstants.Services.Feedback)] = _feedback.Address,
                [TestConstants.ConfigurationKeys.SwaggerEnabled] = bool.TrueString,
                [TestConstants.ConfigurationKeys.SwaggerClientId] = TestConstants.Swagger.ClientId,
                [TestConstants.ConfigurationKeys.SwaggerAudience] = TestConstants.Swagger.Audience
            }));
    }
}
