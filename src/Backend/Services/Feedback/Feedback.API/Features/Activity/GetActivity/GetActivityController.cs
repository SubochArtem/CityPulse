using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedback.API.Features.Activity.GetActivity;

[ApiController]
[Route("api/v1/activity")]
public class GetActivityController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ErrorOr<ActivityPage>> GetActivity(
        [FromQuery] ActivityFilter filter,
        CancellationToken cancellationToken)
    {
        var query = new GetActivityQuery(filter);

        return await sender.Send(query, cancellationToken);
    }
}
