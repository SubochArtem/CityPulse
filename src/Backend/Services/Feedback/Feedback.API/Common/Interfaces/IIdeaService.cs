using ErrorOr;

namespace Feedback.API.Common.Interfaces;

public interface IIdeaService
{
    Task<ErrorOr<Guid>> GetIdeaCityAsync(
        Guid ideaId,
        bool includeOnlyActive,
        CancellationToken cancellationToken = default);
}
