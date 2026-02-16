using System.Net;

namespace Users.Business.Exceptions;

public class Auth0Exception : ExternalServiceException
{
    public Auth0Exception(
        string message,
        HttpStatusCode? statusCode = null,
        Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode? StatusCode { get; }
}
