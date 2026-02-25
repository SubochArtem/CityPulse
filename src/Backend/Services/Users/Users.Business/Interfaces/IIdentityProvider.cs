namespace Users.Business.Interfaces;

public interface IIdentityProvider
{
    public Task BlockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default);

    public Task UnblockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default);

    public Task DeleteUserAsync(
        string identityId,
        CancellationToken cancellationToken = default);
}
