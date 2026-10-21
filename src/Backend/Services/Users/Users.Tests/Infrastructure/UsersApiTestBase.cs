using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Respawn;
using Users.Business.DTOs;
using Users.Business.Interfaces;
using Users.DataAccess;
using Users.DataAccess.DTOs;
using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;
using Users.Tests.Infrastructure.Constants;
using Users.Tests.TestData;
using Xunit;

namespace Users.Tests.Infrastructure;

[Collection(UsersApiCollection.Name)]
[Trait(EnvironmentConstants.Traits.Category, EnvironmentConstants.Traits.Integration)]
public abstract class UsersApiTestBase(IntegrationTestWebAppFactory factory) : IAsyncLifetime
{
    private const string DefaultCityName = "Test City";

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
