using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Polls.Infrastructure.Persistence;
using Polls.Tests.Infrastructure.Constants;
using Respawn;
using Xunit;

namespace Polls.Tests.Infrastructure;

[Collection(PollsApiCollection.Name)]
[Trait(EnvironmentConstants.Traits.Category, EnvironmentConstants.Traits.Integration)]
public abstract class PollsApiTestBase(PollsWebAppFactory factory) : IAsyncLifetime
{
    protected static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

    protected PollsWebAppFactory Factory => factory;

    public async Task InitializeAsync()
    {
        await using var connection = new NpgsqlConnection(factory.ConnectionString);
        await connection.OpenAsync();

        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = [EnvironmentConstants.Respawner.PublicSchema],
            TablesToIgnore = [EnvironmentConstants.Respawner.EfMigrationsHistoryTable]
        });

        await respawner.ResetAsync(connection);

        factory.ResetMocks();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected HttpClient CreateClient(
        IEnumerable<string> permissions,
        Guid? userId = null,
        Guid? cityId = null)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            AuthConstants.Headers.Permissions,
            string.Join(AuthConstants.Headers.PermissionsSeparator, permissions));

        if (userId.HasValue)
            client.DefaultRequestHeaders.Add(AuthConstants.Headers.UserId, userId.Value.ToString());

        if (cityId.HasValue)
            client.DefaultRequestHeaders.Add(AuthConstants.Headers.CityId, cityId.Value.ToString());

        return client;
    }

    protected HttpClient CreateAnonymousClient()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(AuthConstants.Headers.Anonymous, bool.TrueString);

        return client;
    }

    protected async Task SeedAsync(params object[] entities)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.AddRange(entities);
        await context.SaveChangesAsync();
    }

    protected async Task<T> QueryDbAsync<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await query(context);
    }

    protected async Task ExecuteDbAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await action(context);
    }
}

[CollectionDefinition(Name)]
public sealed class PollsApiCollection : ICollectionFixture<PollsWebAppFactory>
{
    public const string Name = "PollsApi";
}
