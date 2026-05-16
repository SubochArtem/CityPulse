using Polls.Application.Common.Interfaces;
using Polls.Domain.Images;

namespace Polls.Infrastructure.Persistence;

public sealed record RepositoryCollection(
    Lazy<ICityRepository> Cities,
    Lazy<IPollRepository> Polls,
    Lazy<IPollScheduleJobRepository> PollScheduleJobs,
    Lazy<IIdeaRepository> Ideas,
    Lazy<IImageRepository<CityImage>> CityImages,
    Lazy<IImageRepository<PollImage>> PollImages,
    Lazy<IImageRepository<IdeaImage>> IdeaImages,
    Lazy<IDeletedImageRepository> DeletedImages);
