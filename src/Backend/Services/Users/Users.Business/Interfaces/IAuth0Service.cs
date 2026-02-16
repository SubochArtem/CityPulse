namespace Users.Business.Interfaces;

public interface IAuth0Service
{
    Task BlockUserAsync(string auth0UserId);

    Task UnblockUserAsync(string auth0UserId);

    Task DeleteUserAsync(string auth0UserId);
}
