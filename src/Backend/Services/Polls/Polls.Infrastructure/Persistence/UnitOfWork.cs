using Microsoft.EntityFrameworkCore.Storage;
using Polls.Application.Common.Interfaces;

namespace Polls.Infrastructure.Persistence;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    Lazy<ICityRepository> cities,
    Lazy<IPollRepository> polls,
    Lazy<IIdeaRepository> ideas) : IUnitOfWork
{
    public ICityRepository Cities => cities.Value;
    public IPollRepository Polls => polls.Value;
    public IIdeaRepository Ideas => ideas.Value;

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return new UnitOfWorkTransaction(transaction);
    }
}
