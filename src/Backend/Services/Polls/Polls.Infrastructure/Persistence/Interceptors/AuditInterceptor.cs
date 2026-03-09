using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Polls.Domain.Common;

namespace Polls.Infrastructure.Persistence.Interceptors;

public sealed class AuditInterceptor(
    ILogger<AuditInterceptor> logger) : SaveChangesInterceptor
{
    private readonly ILogger<AuditInterceptor> _logger = logger;

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

    private void LogEntityChanges(DbContext? context)
    {
        if (context is null || !_logger.IsEnabled(LogLevel.Information))
            return;

        var entries = context.ChangeTracker.Entries<EntityBase>()
            .Where(e => e.State is EntityState.Added
                or EntityState.Modified
                or EntityState.Deleted);

        foreach (var entry in entries)
            _logger.LogInformation(
                "Entity: {Entity}, Id: {Id}, State: {State}",
                entry.Metadata.ClrType.Name,
                entry.Entity.Id,
                entry.State);
    }
}
