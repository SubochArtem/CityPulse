using CityPulse.Common.Api.Swagger;
using Microsoft.AspNetCore.Mvc;
using Users.Business.Configurations;
using Users.Business.DTOs;
using Users.Business.Interfaces;
using Users.Presentation.Extensions;

namespace Users.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class Auth0WebhookController(IIdentityProviderWebhookService webhookService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<GetUserDto?> HandleAsync(CancellationToken cancellationToken)
    {
        var rawBody = await HttpContext.Request.ReadRawBodyAsync();
        var signature = HttpContext
            .Request
            .Headers[IdentityProviderConstants.WebhookSignatureHeader]
            .ToString();

        return await webhookService.HandleAsync(rawBody, signature, cancellationToken);
    }
}
