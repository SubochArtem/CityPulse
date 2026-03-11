using System.Text;

namespace Users.Presentation.Extensions;

public static class WebhookRequestExtensions
{
    public static async Task<string> ReadRawBodyAsync(this HttpRequest request)
    {
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }
}
