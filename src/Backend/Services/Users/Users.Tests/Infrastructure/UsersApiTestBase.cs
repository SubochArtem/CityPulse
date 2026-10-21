using Moq;
using Npgsql;
using Respawn;
using Users.Business.Interfaces;
using Users.Tests.Infrastructure.Constants;
using Xunit;

namespace Users.Tests.Infrastructure;

[Collection(UsersApiCollection.Name)]
[Trait(EnvironmentConstants.Traits.Category, EnvironmentConstants.Traits.Integration)]
public abstract class UsersApiTestBase(IntegrationTestWebAppFactory factory) : IAsyncLifetime
{
    private Respawner _respawner = null!;

    protected HttpClient Client { get; private set; } = null!;
    
    protected Mock<IIdentityProvider> IdentityProviderMock => factory.IdentityProviderMock;

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

    public async Task DisposeAsync()
    {
        IdentityProviderMock.Invocations.Clear();

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
