using CityPulse.Common.Api.Extensions;
using ErrorOr;
using Feedback.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.API.Features.Feedback.EditComment;

[ApiController]
[Route("api/v1/admin/feedback")]
public class AdminEditCommentController(ISender sender) : ControllerBase
{
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Feedback.UpdateAny)]
    public async Task<ErrorOr<FeedbackResponse>> EditComment(
        Guid id,
        EditCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EditCommentCommand(
            UserId: User.GetUserId(),
            Id: id,
            Text: request.Text,
            BypassRestrictions: true);

        return await sender.Send(command, cancellationToken);
    }
}
