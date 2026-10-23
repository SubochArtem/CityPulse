using CityPulse.Contracts.Querying.Pagination;
using ErrorOr;
using MediatR;

namespace Feedback.API.Features.Feedback.GetFilteredFeedback;

public record GetFilteredFeedbackQuery(
    FeedbackFilter Filter) : IRequest<ErrorOr<PagedList<FeedbackResponse>>>;
