using Polls.Application.Common.Interfaces;

namespace Polls.Infrastructure.Persistence;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    ICityRepository cityRepository,
    IPollRepository pollRepository,
    IIdeaRepository ideaRepository) : IUnitOfWork
{
    public ICityRepository Cities { get; } = cityRepository;
    public IPollRepository Polls { get; } = pollRepository;
    public IIdeaRepository Ideas { get; } = ideaRepository;

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
