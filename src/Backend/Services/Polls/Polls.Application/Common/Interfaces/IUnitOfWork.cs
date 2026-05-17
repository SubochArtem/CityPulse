using Polls.Domain.Images;

namespace Polls.Application.Common.Interfaces;

public interface IUnitOfWork
{
    ICityRepository Cities { get; }
    IPollRepository Polls { get; }
    IIdeaRepository Ideas { get; }
    IPollScheduleJobRepository PollScheduleJobs { get; }
    IImageRepository<CityImage> CityImages { get; }
    IImageRepository<PollImage> PollImages { get; }
    IImageRepository<IdeaImage> IdeaImages { get; }
    IDeletedImageRepository DeletedImages { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
