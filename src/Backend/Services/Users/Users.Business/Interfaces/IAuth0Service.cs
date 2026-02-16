namespace Users.Business.Interfaces;

public interface IAuth0Service
{
    Task BlockUserAsync(
        string auth0UserId,
        CancellationToken cancellationToken = default);

    Task UnblockUserAsync(
        string auth0UserId,
        CancellationToken cancellationToken = default);

    Task DeleteUserAsync(
        string auth0UserId,
        CancellationToken cancellationToken = default);
}
