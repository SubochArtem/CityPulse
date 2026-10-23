using CityPulse.Contracts.Querying.Pagination;
using ErrorOr;
using Feedback.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.API.Features.Feedback.GetFilteredFeedback;

[ApiController]
[Route("api/v1/feedback")]
public class GetFilteredFeedbackController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.Feedback.ReadAny)]
    public async Task<ErrorOr<PagedList<FeedbackResponse>>> GetFilteredFeedback(
        [FromQuery] FeedbackFilter filter,
        CancellationToken cancellationToken)
    {
        var query = new GetFilteredFeedbackQuery(filter);

        return await sender.Send(query, cancellationToken);
    }
}
