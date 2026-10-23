using CityPulse.Common.Api.Extensions;
using ErrorOr;
using Feedback.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.API.Features.Feedback.EditComment;

[ApiController]
[Route("api/v1/feedback")]
public class UserEditCommentController(ISender sender) : ControllerBase
{
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Feedback.UpdateOwn)]
    public async Task<ErrorOr<FeedbackResponse>> EditComment(
        Guid id,
        EditCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EditCommentCommand(
            UserId: User.GetUserId(),
            Id: id,
            Text: request.Text,
            BypassRestrictions: false);

        return await sender.Send(command, cancellationToken);
    }
}
