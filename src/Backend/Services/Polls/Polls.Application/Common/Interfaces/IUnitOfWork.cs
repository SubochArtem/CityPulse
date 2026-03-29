namespace Polls.Application.Common.Interfaces;

public interface IUnitOfWork
{
    ICityRepository Cities { get; }
    IPollRepository Polls { get; }
    IIdeaRepository Ideas { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
