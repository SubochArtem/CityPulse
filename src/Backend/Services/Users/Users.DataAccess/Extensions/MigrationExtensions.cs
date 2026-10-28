using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Users.DataAccess.Settings;

namespace Users.DataAccess.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsIfEnabledAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var settings = services.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        if (!settings.ApplyMigrationsOnStartup)
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
