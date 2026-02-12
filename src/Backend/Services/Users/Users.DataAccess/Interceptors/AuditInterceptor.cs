using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Users.DataAccess.Entities;

namespace Users.DataAccess.Interceptors;

public sealed class AuditInterceptor(
    ILogger<AuditInterceptor> logger
) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        LogEntityChanges(eventData.Context, logger);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        LogEntityChanges(eventData.Context, logger);
        return ValueTask.FromResult(result);
    }

    private static void LogEntityChanges(DbContext? context, ILogger logger)
    {
        if (context is null || !logger.IsEnabled(LogLevel.Information))
            return;

        var entries = context.ChangeTracker.Entries<EntityBase>()
            .Where(e =>
                e.State is EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted);

        foreach (var entry in entries)
            logger.LogInformation(
                "Entity: {Entity}, Id: {Id}, State: {State}",
                entry.Metadata.ClrType.Name,
                entry.Entity.Id,
                entry.State
            );
    }
}
