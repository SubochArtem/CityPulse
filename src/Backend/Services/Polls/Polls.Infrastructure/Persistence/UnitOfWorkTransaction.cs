using Microsoft.EntityFrameworkCore.Storage;
using Polls.Application.Common.Interfaces;

namespace Polls.Infrastructure.Persistence;

public sealed class UnitOfWorkTransaction(IDbContextTransaction transaction) : 
    IUnitOfWorkTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default)
        => transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => transaction.RollbackAsync(cancellationToken);

    public void Dispose() 
        => transaction.Dispose();

    public ValueTask DisposeAsync() 
        => transaction.DisposeAsync();
}
