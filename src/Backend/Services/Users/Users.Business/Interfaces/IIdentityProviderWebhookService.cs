namespace Users.Business.Interfaces;

public interface IIdentityProviderWebhookService
{
    public Task HandleAsync(
        string rawBody,
        string signature,
        CancellationToken cancellationToken = default);
}
