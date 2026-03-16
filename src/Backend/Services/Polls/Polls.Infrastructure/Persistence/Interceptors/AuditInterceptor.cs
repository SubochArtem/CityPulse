using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Polls.Domain.Common;
using Serilog;

namespace Polls.Infrastructure.Persistence.Interceptors;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private static readonly ILogger Logger = Log
        .ForContext<AuditInterceptor>();

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        LogEntityChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        LogEntityChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void LogEntityChanges(DbContext? context)
    {
        if (context is null)
            return;

        var entries = context.ChangeTracker.Entries<EntityBase>()
            .Where(e => e.State is EntityState.Added
                or EntityState.Modified
                or EntityState.Deleted);

        foreach (var entry in entries)
            Logger.Information(
                "Entity: {Entity}, Id: {Id}, State: {State}",
                entry.Metadata.ClrType.Name,
                entry.Entity.Id,
                entry.State);
    }
}
