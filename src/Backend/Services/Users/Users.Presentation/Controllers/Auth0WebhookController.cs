using Microsoft.AspNetCore.Mvc;
using Users.Business.Configurations;
using Users.Business.Interfaces;
using Users.Presentation.Extensions;

namespace Users.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class Auth0WebhookController(IIdentityProviderWebhookService webhookService) : ControllerBase
{
    private readonly IIdentityProviderWebhookService _webhookService = webhookService;

    [HttpPost]
    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        var rawBody = await HttpContext.Request.ReadRawBodyAsync();
        var signature = HttpContext
            .Request
            .Headers[IdentityProviderConstants.WebhookSignatureHeader]
            .ToString();

        await _webhookService.HandleAsync(rawBody, signature, cancellationToken);
    }
}
