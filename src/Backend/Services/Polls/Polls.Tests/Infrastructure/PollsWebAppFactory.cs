using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Infrastructure.Messaging;
using Polls.Infrastructure.Persistence;
using Polls.Tests.Infrastructure.Constants;
using Testcontainers.PostgreSql;
using Xunit;

namespace Polls.Tests.Infrastructure;

public sealed class PollsWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(EnvironmentConstants.Postgres.Image)
        .WithDatabase(EnvironmentConstants.Postgres.Database)
        .WithUsername(EnvironmentConstants.Postgres.Username)
        .WithPassword(EnvironmentConstants.Postgres.Password)
        .Build();

    public Mock<IImageStorageService> ImageStorage { get; } = new();

    public Mock<IPollScheduler> PollScheduler { get; } = new();

    public string ConnectionString => _postgresContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.MigrateAsync();

        ResetMocks();
    }

    public void ResetMocks()
    {
        ImageStorage.Reset();
        ImageStorage
            .Setup(x => x.GetUrl(It.IsAny<string>()))
            .Returns<string>(fileName => EnvironmentConstants.ImageStorage.UrlPrefix + fileName);
        ImageStorage
            .Setup(x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ImageFile> files, CancellationToken _) =>
                (IReadOnlyList<string>)files
                    .Select(file => $"{EnvironmentConstants.ImageStorage.UploadedPrefix}{Guid.NewGuid():N}-{file.FileName}")
                    .ToList());
        ImageStorage
            .Setup(x => x.DeleteByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        PollScheduler.Reset();
        PollScheduler
            .Setup(x => x.ScheduleAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        PollScheduler
            .Setup(x => x.CancelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentConstants.Environment);

        var configValues = new Dictionary<string, string?>
        {
            [ConfigurationKeys.Database.ConnectionString] = ConnectionString,

            [ConfigurationKeys.RabbitMq.Host] = EnvironmentConstants.RabbitMq.Host,
            [ConfigurationKeys.RabbitMq.Port] = EnvironmentConstants.RabbitMq.Port,
            [ConfigurationKeys.RabbitMq.Username] = EnvironmentConstants.RabbitMq.Username,
            [ConfigurationKeys.RabbitMq.Password] = EnvironmentConstants.RabbitMq.Password,
            [ConfigurationKeys.RabbitMq.VirtualHost] = EnvironmentConstants.RabbitMq.VirtualHost,

            [ConfigurationKeys.ImageStorage.Endpoint] = EnvironmentConstants.ImageStorage.Endpoint,
            [ConfigurationKeys.ImageStorage.AccessKey] = EnvironmentConstants.ImageStorage.AccessKey,
            [ConfigurationKeys.ImageStorage.SecretKey] = EnvironmentConstants.ImageStorage.SecretKey,
            [ConfigurationKeys.ImageStorage.BucketName] = EnvironmentConstants.ImageStorage.BucketName,
            [ConfigurationKeys.ImageStorage.UseSsl] = EnvironmentConstants.ImageStorage.UseSsl,

            [ConfigurationKeys.Auth0.Domain] = EnvironmentConstants.Auth0.Domain,
            [ConfigurationKeys.Auth0.Audience] = EnvironmentConstants.Auth0.Audience,
            [ConfigurationKeys.Auth0.ClientId] = EnvironmentConstants.Auth0.ClientId
        };

        foreach (var (key, value) in configValues)
            builder.UseSetting(key, value);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveMassTransitHostedService();
            services.AddMassTransitTestHarness(configurator =>
            {
                configurator.SetKebabCaseEndpointNameFormatter();
                configurator.AddEntityFrameworkOutbox<ApplicationDbContext>(outbox =>
                {
                    outbox.UsePostgres();
                    outbox.UseBusOutbox();
                });
                configurator.AddConsumer<UserStatusChangedConsumer>();
            });

            services.RemoveAll<IImageStorageService>();
            services.AddSingleton(ImageStorage.Object);

            services.RemoveAll<IPollScheduler>();
            services.AddSingleton(PollScheduler.Object);

            services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
        });
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
