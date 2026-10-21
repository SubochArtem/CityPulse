using System.Text;
using System.Text.Json;
using Bogus;
using CityPulse.Contracts.Events.Users;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Respawn;
using Users.Business.Configurations;
using Users.Business.DTOs;
using Users.Business.Interfaces;
using Users.DataAccess;
using Users.DataAccess.DTOs;
using Users.DataAccess.Entities;
using Users.DataAccess.Entities.Enums;
using Users.DataAccess.Interfaces;
using Users.Tests.Infrastructure.Constants;
using Users.Tests.TestData;
using Xunit;

namespace Users.Tests.Infrastructure;

[Collection(UsersApiCollection.Name)]
[Trait(EnvironmentConstants.Traits.Category, EnvironmentConstants.Traits.Integration)]
public abstract class UsersApiTestBase(IntegrationTestWebAppFactory factory) : IAsyncLifetime
{
    private const string WebhookRoute = "api/v1/Auth0Webhook";
    private const string JsonContentType = "application/json";
    private const string DefaultCityName = "Test City";
    private const string OutboxEnvelopeMessageProperty = "message";

    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Faker<User> UserFaker = UserFakers.Users();

    private Respawner _respawner = null!;

    protected HttpClient Client { get; private set; } = null!;
    
    protected Mock<IIdentityProvider> IdentityProviderMock => factory.IdentityProviderMock;

    protected Mock<ICityService> CityServiceMock => factory.CityServiceMock;

    public async Task InitializeAsync()
    {
        Client = factory.CreateClient();

        await using var connection = new NpgsqlConnection(factory.GetConnectionString());
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = [EnvironmentConstants.Respawner.PublicSchema],
            TablesToIgnore = [EnvironmentConstants.Respawner.EfMigrationsHistoryTable]
        });
    }

    protected async Task<User> SeedUserAsync(User? user = null)
    {
        user ??= UserFaker.Generate();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    protected HttpClient CreateClient(IEnumerable<string> permissions, string? internalUserId = null)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            AuthConstants.Headers.Permissions,
            string.Join(AuthConstants.Headers.PermissionsSeparator, permissions));

        if (internalUserId is not null)
        {
            client.DefaultRequestHeaders.Add(AuthConstants.Headers.InternalUserId, internalUserId);
        }

        return client;
    }

    protected HttpClient CreateAnonymousClient()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(AuthConstants.Headers.Anonymous, bool.TrueString);

        return client;
    }

    protected Task<User> SeedUserAsync(UserAccessStatus accessStatus)
    {
        var user = UserFaker.Generate();
        user.AccessStatus = accessStatus;

        return SeedUserAsync(user);
    }

    protected void SetupCity(Guid cityId, CityStatus status) =>
        CityServiceMock
            .Setup(x => x.GetCityAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CityDto { Id = cityId, Name = DefaultCityName, Status = status });

    protected void VerifyIdentityProviderNeverUpdated() =>
        IdentityProviderMock.Verify(
            x => x.UpdateUserProfileAsync(
                It.IsAny<string>(),
                It.IsAny<UpdateUserProfileDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected async Task<List<UserStatusChangedEvent>> GetOutboxStatusChangedEventsAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var bodies = await dbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .Where(m => m.MessageType.Contains(nameof(UserStatusChangedEvent)))
            .Select(m => m.Body)
            .ToListAsync();

        return bodies
            .Select(body => JsonDocument.Parse(body).RootElement
                .GetProperty(OutboxEnvelopeMessageProperty)
                .Deserialize<UserStatusChangedEvent>(WebJsonOptions)!)
            .ToList();
    }

    protected async Task<HttpResponseMessage> PostWebhookAsync(string body, string? signature)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, WebhookRoute)
        {
            Content = new StringContent(body, Encoding.UTF8, JsonContentType)
        };

        if (signature is not null)
        {
            request.Headers.Add(IdentityProviderConstants.WebhookSignatureHeader, signature);
        }

        return await Client.SendAsync(request);
    }

    protected async Task<User?> GetUserByIdentityIdFromDbAsync(string identityId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.IdentityId == identityId);
    }

    protected async Task<int> CountUsersInDbAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Users.CountAsync();
    }

    protected async Task<User?> GetUserFromDbAsync(Guid id)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task DisposeAsync()
    {
        IdentityProviderMock.Reset();
        CityServiceMock.Reset();

        await using var connection = new NpgsqlConnection(factory.GetConnectionString());
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }
}

[CollectionDefinition(Name)]
public sealed class UsersApiCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
    public const string Name = "UsersApi";
}
