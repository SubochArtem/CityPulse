using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Gateway.Tests.Infrastructure;

public sealed class StubBackend : IAsyncDisposable
{
    private readonly WebApplication _app;

    private StubBackend(WebApplication app, string address)
    {
        _app = app;
        Address = address;
    }

    public string Address { get; }

    public static async Task<StubBackend> StartAsync(string name)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(TestConstants.LoopbackUrl);
        builder.Logging.ClearProviders();

        var app = builder.Build();
        app.Run(async context =>
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();

            await context.Response.WriteAsJsonAsync(new BackendEcho(
                name,
                context.Request.Method,
                context.Request.Path.Value ?? string.Empty,
                context.Request.QueryString.Value ?? string.Empty,
                context.Request.Headers.Authorization.ToString(),
                body));
        });

        await app.StartAsync();

        return new StubBackend(app, app.Urls.First());
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
    }
}
