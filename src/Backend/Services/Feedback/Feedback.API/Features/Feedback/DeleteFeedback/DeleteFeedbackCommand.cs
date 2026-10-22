using ErrorOr;
using MediatR;

namespace Feedback.API.Features.Feedback.DeleteFeedback;

public record DeleteFeedbackCommand(
    Guid UserId,
    Guid Id,
    bool CanDeleteAny,
    bool CanDeleteOwn) : IRequest<ErrorOr<Deleted>>;
