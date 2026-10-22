using ErrorOr;
using MediatR;

namespace Feedback.API.Features.Feedback.AddFeedback;

public record AddFeedbackCommand(
    Guid UserId,
    Guid IdeaId,
    string? Text) : IRequest<ErrorOr<FeedbackResponse>>;
