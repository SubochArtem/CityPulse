namespace Users.Business.Exceptions;

public class UserAlreadyExistsException(string auth0UserId)
    : Exception($"User with Auth0 ID '{auth0UserId}' already exists.")
{
    public string Auth0UserId { get; } = auth0UserId;
}
