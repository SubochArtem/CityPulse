using CityPulse.Common.Api.Extensions;
using ErrorOr;
using Feedback.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.API.Features.Feedback.DeleteFeedback;

[ApiController]
[Route("api/v1/feedback")]
public class UserDeleteFeedbackController(ISender sender) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Feedback.DeleteOwn)]
    public async Task<ErrorOr<Deleted>> DeleteFeedback(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFeedbackCommand(
            UserId: User.GetUserId(),
            Id: id,
            BypassRestrictions: false);

        return await sender.Send(command, cancellationToken);
    }
}
