using Npgsql;
using Polls.Tests.Infrastructure.Constants;
using Testcontainers.PostgreSql;
using Xunit;

namespace Polls.Tests.Infrastructure;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(EnvironmentConstants.Postgres.Image)
        .WithDatabase(EnvironmentConstants.Postgres.Database)
        .WithUsername(EnvironmentConstants.Postgres.Username)
        .WithPassword(EnvironmentConstants.Postgres.Password)
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public string CreateConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = $"{EnvironmentConstants.Postgres.Database}_{Guid.NewGuid():N}"
        };

        return builder.ConnectionString;
    }
}
