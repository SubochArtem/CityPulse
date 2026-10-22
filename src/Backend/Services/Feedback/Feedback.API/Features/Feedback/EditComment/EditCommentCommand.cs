using ErrorOr;
using MediatR;

namespace Feedback.API.Features.Feedback.EditComment;

public record EditCommentCommand(
    Guid UserId,
    Guid Id,
    string Text) : IRequest<ErrorOr<FeedbackResponse>>;
