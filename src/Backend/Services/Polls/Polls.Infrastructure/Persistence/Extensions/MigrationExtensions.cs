using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polls.Infrastructure.Persistence.Options;

namespace Polls.Infrastructure.Persistence.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsIfEnabledAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var options = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        if (!options.ApplyMigrationsOnStartup)
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
