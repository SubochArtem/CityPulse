namespace Users.Business.Exceptions;

public abstract class ExternalServiceException : Exception
{
    protected ExternalServiceException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
