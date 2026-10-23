using System.Net;

namespace Users.Tests.Infrastructure;

public sealed class StubHttpMessageHandler(
    Func<RecordedRequest, HttpResponseMessage> responder) : HttpMessageHandler
{
    private readonly List<RecordedRequest> _requests = [];

    public IReadOnlyList<RecordedRequest> Requests => _requests;

    public static StubHttpMessageHandler Returning(HttpStatusCode statusCode, string? content = null) =>
        new(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content ?? string.Empty)
        });

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        var recorded = new RecordedRequest(
            request.Method,
            request.RequestUri,
            body,
            request.Headers.Authorization?.ToString());

        _requests.Add(recorded);

        return responder(recorded);
    }
}

public sealed record RecordedRequest(
    HttpMethod Method,
    Uri? Uri,
    string? Body,
    string? Authorization);
