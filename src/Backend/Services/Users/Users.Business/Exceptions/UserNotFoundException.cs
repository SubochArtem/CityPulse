namespace Users.Business.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(Guid userId)
        : base($"User with ID '{userId}' was not found.")
    {
        UserId = userId;
    }

    public UserNotFoundException(string auth0UserId)
        : base($"User with Auth0 ID '{auth0UserId}' was not found.")
    {
        Auth0UserId = auth0UserId;
    }

    public Guid? UserId { get; }

    public string? Auth0UserId { get; }
}
