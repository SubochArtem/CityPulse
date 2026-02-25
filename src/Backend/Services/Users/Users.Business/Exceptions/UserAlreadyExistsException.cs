namespace Users.Business.Exceptions;

public class UserAlreadyExistsException(string identityId, string identityProvider)
    : Exception($"User with Identity ID '{identityId}' already exists in provider '{identityProvider}'.")
{
    public string IdentityId { get; } = identityId;
    public string IdentityProvider { get; } = identityProvider;
}
