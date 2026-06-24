namespace Users.Business.Exceptions;

public class UnsupportedWebhookEventException(string? eventName)
    : Exception($"Webhook event '{eventName ?? "Unknown"}' is not supported")
{
    public string? EventName { get; } = eventName;
}
