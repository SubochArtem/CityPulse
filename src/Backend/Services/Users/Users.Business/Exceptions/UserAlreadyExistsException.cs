namespace Users.Business.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string auth0UserId)
        : base($"User with Auth0 ID '{auth0UserId}' already exists.")
    {
        Auth0UserId = auth0UserId;
    }

    public string Auth0UserId { get; }
}
