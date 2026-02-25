namespace Users.Business.Exceptions;

public abstract class ExternalServiceException(string message, Exception? inner = null)
    : Exception(message, inner);
