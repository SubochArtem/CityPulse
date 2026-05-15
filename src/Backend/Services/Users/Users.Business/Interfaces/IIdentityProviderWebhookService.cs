using Users.Business.DTOs;

namespace Users.Business.Interfaces;

public interface IIdentityProviderWebhookService
{
    public Task<GetUserDto?> HandleAsync(
        string rawBody,
        string signature,
        CancellationToken cancellationToken = default);
}
