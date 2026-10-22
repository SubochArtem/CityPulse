using CityPulse.Common.Api.Extensions;
using ErrorOr;
using Feedback.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.API.Features.Feedback.AddFeedback;

[ApiController]
[Route("api/v1/ideas/{ideaId:guid}/feedback")]
public class AddFeedbackController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Permissions.Feedback.CreateAny)]
    public async Task<ErrorOr<FeedbackResponse>> AddFeedback(
        Guid ideaId,
        AddFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddFeedbackCommand(
            UserId: User.GetUserId(),
            IdeaId: ideaId,
            Text: request.Text);

        return await sender.Send(command, cancellationToken);
    }
}
