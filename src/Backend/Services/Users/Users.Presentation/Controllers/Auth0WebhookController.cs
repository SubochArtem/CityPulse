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
    private const string JsonContentType = "application/json";
    private const string ProblemContentType = "application/problem+json";

    [HttpPost]
    [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK, JsonContentType)]
    [ProducesResponseType(
        typeof(ApiProblemDetails),
        StatusCodes.Status400BadRequest,
        ProblemContentType,
        JsonContentType)]
    [ProducesResponseType(
        typeof(ApiProblemDetails),
        StatusCodes.Status401Unauthorized,
        ProblemContentType,
        JsonContentType)]
    public async Task<GetUserDto?> HandleAsync(
        [FromHeader(Name = IdentityProviderConstants.WebhookSignatureHeader)] string? signature,
        CancellationToken cancellationToken)
    {
        var rawBody = await HttpContext.Request.ReadRawBodyAsync();

        return await webhookService.HandleAsync(rawBody, signature ?? string.Empty, cancellationToken);
    }
}
