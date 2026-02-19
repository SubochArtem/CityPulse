namespace Users.Business.Exceptions;

public class Auth0Exception : ExternalServiceException
{
    public Auth0Exception(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
