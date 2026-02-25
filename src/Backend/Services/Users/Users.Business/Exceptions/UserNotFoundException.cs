namespace Users.Business.Exceptions;


public class UserNotFoundException(string identityId, string identityProvider)
    : Exception($"User with Identity ID '{identityId}' was not found in provider '{identityProvider}'.")
{
    public string IdentityId { get; } = identityId;
    public string IdentityProvider { get; } = identityProvider;
}
