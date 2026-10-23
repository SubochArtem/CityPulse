using ErrorOr;

namespace Feedback.API.Common.Services;

public interface IIdeaService
{
    Task<ErrorOr<Guid>> GetIdeaCityAsync(
        Guid ideaId,
        bool includeOnlyActive,
        CancellationToken cancellationToken = default);
}
