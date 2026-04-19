using Polls.Application.Common.Interfaces;
using Polls.Domain.Images;

namespace Polls.Infrastructure.Persistence;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    Lazy<ICityRepository> cities,
    Lazy<IPollRepository> polls,
    Lazy<IPollScheduleJobRepository> pollScheduleJobs,
    Lazy<IIdeaRepository> ideas,
    Lazy<IImageRepository<CityImage>> cityImages,
    Lazy<IImageRepository<PollImage>> pollImages,
    Lazy<IImageRepository<IdeaImage>> ideaImages,
    Lazy<IDeletedImageRepository> deletedImages) : IUnitOfWork
{
    public ICityRepository Cities => cities.Value;
    public IPollRepository Polls => polls.Value;
    public IIdeaRepository Ideas => ideas.Value;
    public IPollScheduleJobRepository PollScheduleJobs => pollScheduleJobs.Value;
    public IImageRepository<CityImage> CityImages => cityImages.Value;
    public IImageRepository<PollImage> PollImages => pollImages.Value;
    public IImageRepository<IdeaImage> IdeaImages => ideaImages.Value;
    public IDeletedImageRepository DeletedImages => deletedImages.Value;

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
