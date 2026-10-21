using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Users.Business.Interfaces;
using Users.DataAccess;
using Users.Tests.Infrastructure.Constants;
using Xunit;

namespace Users.Tests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(EnvironmentConstants.Postgres.Image)
        .WithDatabase(EnvironmentConstants.Postgres.Database)
        .WithUsername(EnvironmentConstants.Postgres.Username)
        .WithPassword(EnvironmentConstants.Postgres.Password)
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder(EnvironmentConstants.RabbitMq.Image)
        .Build();
    
    public Mock<IIdentityProvider> IdentityProviderMock { get; } = new();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentConstants.Environment);

        var configValues = new Dictionary<string, string?>
        {
            [ConfigurationKeys.ConnectionStrings.DefaultConnection] = _postgresContainer.GetConnectionString(),

            [ConfigurationKeys.RabbitMq.Host] = _rabbitMqContainer.Hostname,
            [ConfigurationKeys.RabbitMq.Port] = _rabbitMqContainer.GetMappedPublicPort(EnvironmentConstants.RabbitMq.DefaultPort).ToString(),
            [ConfigurationKeys.RabbitMq.Username] = EnvironmentConstants.RabbitMq.Username,
            [ConfigurationKeys.RabbitMq.Password] = EnvironmentConstants.RabbitMq.Password,
            [ConfigurationKeys.RabbitMq.VirtualHost] = EnvironmentConstants.RabbitMq.VirtualHost,

            [ConfigurationKeys.Grpc.CitiesServiceUrl] = EnvironmentConstants.Grpc.DummyCitiesServiceUrl,

            [ConfigurationKeys.Auth0.Domain] = EnvironmentConstants.Auth0.Domain,
            [ConfigurationKeys.Auth0.ClientId] = EnvironmentConstants.Auth0.ClientId,
            [ConfigurationKeys.Auth0.ClientSecret] = EnvironmentConstants.Auth0.ClientSecret,
            [ConfigurationKeys.Auth0.Audience] = EnvironmentConstants.Auth0.Audience,
            [ConfigurationKeys.Auth0.ManagementApiClientId] = EnvironmentConstants.Auth0.ManagementApiClientId,
            [ConfigurationKeys.Auth0.ManagementApiSecret] = EnvironmentConstants.Auth0.ManagementApiSecret,
            [ConfigurationKeys.Auth0.ManagementApiAudience] = EnvironmentConstants.Auth0.ManagementApiAudience,
            [ConfigurationKeys.Auth0.WebhookSecret] = EnvironmentConstants.Auth0.WebhookSecret
        };

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(configValues);
        });

        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));

            var identityDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IIdentityProvider));
            if (identityDescriptor != null)
            {
                services.Remove(identityDescriptor);
            }

            services.AddSingleton<IIdentityProvider>(IdentityProviderMock.Object);

            services.AddAuthentication(defaultScheme: TestAuthenticationHandler.SchemeName)
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName, _ => { });
        });
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    public string GetConnectionString() => _postgresContainer.GetConnectionString();
}
