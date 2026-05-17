using Polls.Application.Common.Interfaces;
using Polls.Domain.Images;

namespace Polls.Infrastructure.Persistence;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    RepositoryCollection repositories) : IUnitOfWork
{
    public ICityRepository Cities => repositories.Cities.Value;
    public IPollRepository Polls => repositories.Polls.Value;
    public IIdeaRepository Ideas => repositories.Ideas.Value;
    public IPollScheduleJobRepository PollScheduleJobs => repositories.PollScheduleJobs.Value;
    public IImageRepository<CityImage> CityImages => repositories.CityImages.Value;
    public IImageRepository<PollImage> PollImages => repositories.PollImages.Value;
    public IImageRepository<IdeaImage> IdeaImages => repositories.IdeaImages.Value;
    public IDeletedImageRepository DeletedImages => repositories.DeletedImages.Value;

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
