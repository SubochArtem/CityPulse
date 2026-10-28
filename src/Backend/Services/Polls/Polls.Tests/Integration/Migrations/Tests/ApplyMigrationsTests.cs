using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polls.Infrastructure.Persistence;
using Polls.Infrastructure.Persistence.Extensions;
using Polls.Infrastructure.Persistence.Options;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Xunit;

namespace Polls.Tests.Integration.Migrations.Tests;

[Trait(EnvironmentConstants.Traits.Category, EnvironmentConstants.Traits.Integration)]
public sealed class ApplyMigrationsTests(PostgresContainerFixture postgres) : IClassFixture<PostgresContainerFixture>
{
    private ServiceProvider BuildServices(bool applyMigrationsOnStartup)
    {
        var connectionString = postgres.CreateConnectionString();

        return new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString))
            .AddSingleton(Options.Create(new DatabaseOptions
            {
                ConnectionString = connectionString,
                ApplyMigrationsOnStartup = applyMigrationsOnStartup
            }))
            .BuildServiceProvider();
    }

    private static async Task<(IReadOnlyList<string> Applied, IReadOnlyList<string> All)> ReadMigrationsAsync(
        ServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database;

        var applied = (await database.GetAppliedMigrationsAsync()).ToList();
        var all = database.GetMigrations().ToList();

        return (applied, all);
    }

    [Fact]
    public async Task ApplyMigrationsIfEnabledAsync_FlagEnabled_ShouldApplyAllMigrations()
    {
        await using var services = BuildServices(applyMigrationsOnStartup: true);

        await services.ApplyMigrationsIfEnabledAsync();

        var (applied, all) = await ReadMigrationsAsync(services);
        Assert.NotEmpty(all);
        Assert.Equal(all, applied);
    }

    [Fact]
    public async Task ApplyMigrationsIfEnabledAsync_FlagDisabled_ShouldNotApplyMigrations()
    {
        await using var services = BuildServices(applyMigrationsOnStartup: false);

        await services.ApplyMigrationsIfEnabledAsync();

        var (applied, all) = await ReadMigrationsAsync(services);
        Assert.NotEmpty(all);
        Assert.Empty(applied);
    }

    [Fact]
    public async Task ApplyMigrationsIfEnabledAsync_CalledTwice_ShouldKeepAllMigrationsApplied()
    {
        await using var services = BuildServices(applyMigrationsOnStartup: true);

        await services.ApplyMigrationsIfEnabledAsync();
        await services.ApplyMigrationsIfEnabledAsync();

        var (applied, all) = await ReadMigrationsAsync(services);
        Assert.Equal(all, applied);
    }
}
