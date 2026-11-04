using ErrorOr;
using MediatR;

namespace Feedback.API.Features.Activity.GetActivity;

public record GetActivityQuery(ActivityFilter Filter) : IRequest<ErrorOr<ActivityPage>>;
