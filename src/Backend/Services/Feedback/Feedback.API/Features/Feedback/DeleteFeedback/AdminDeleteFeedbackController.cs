using CityPulse.Common.Api.Extensions;
using ErrorOr;
using Feedback.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.API.Features.Feedback.DeleteFeedback;

[ApiController]
[Route("api/v1/admin/feedback")]
public class AdminDeleteFeedbackController(ISender sender) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Feedback.DeleteAny)]
    public async Task<ErrorOr<Deleted>> DeleteFeedback(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFeedbackCommand(
            UserId: User.GetUserId(),
            Id: id,
            BypassRestrictions: true);

        return await sender.Send(command, cancellationToken);
    }
}
