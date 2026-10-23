using ErrorOr;
using MediatR;

namespace Feedback.API.Features.Feedback.AddFeedback;

public record AddFeedbackCommand(
    Guid UserId,
    Guid UserCityId,
    Guid IdeaId,
    string? Text,
    bool BypassRestrictions) : IRequest<ErrorOr<FeedbackResponse>>;
