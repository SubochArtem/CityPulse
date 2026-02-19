namespace Users.Business.Exceptions;

public class Auth0Exception(string message, Exception? inner = null)
    : ExternalServiceException(message, inner);
