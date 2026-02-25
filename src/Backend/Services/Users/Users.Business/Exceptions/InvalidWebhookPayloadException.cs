namespace Users.Business.Exceptions;

public class InvalidWebhookPayloadException(string message)
    : Exception(message);
