namespace Users.Business.Interfaces;

public interface IIdentityProvider
{
    Task BlockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default);

    Task UnblockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default);

    Task DeleteUserAsync(
        string identityId,
        CancellationToken cancellationToken = default);
}
