using Gateway.API.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;

namespace Gateway.API.Bff;

public sealed class BffTokenForwardingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, SessionTokenProvider tokenProvider)
    {
        var request = context.Request;
        if (!request.Headers.ContainsKey(HeaderNames.Authorization))
        {
            var authenticated = await context.AuthenticateAsync(BffDefaults.CookieScheme);
            if (authenticated.Succeeded && !await TryAttachTokenAsync(context, tokenProvider, authenticated.Properties!))
            {
                return;
            }
        }

        request.Headers.Remove(HeaderNames.Cookie);
        await next(context);
    }

    private static async Task<bool> TryAttachTokenAsync(
        HttpContext context,
        SessionTokenProvider tokenProvider,
        AuthenticationProperties properties)
    {
        if (!IsSafeMethod(context.Request.Method) && !HasCsrfHeader(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            return false;
        }

        var token = await tokenProvider.GetAccessTokenAsync(context, properties);
        if (token.Status == SessionTokenStatus.Expired)
        {
            await context.SignOutAsync(BffDefaults.CookieScheme);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return false;
        }

        if (token.Status == SessionTokenStatus.Unavailable)
        {
            context.Response.StatusCode = StatusCodes.Status502BadGateway;

            return false;
        }

        context.Request.Headers.Authorization = $"{BffDefaults.BearerScheme} {token.AccessToken}";

        return true;
    }

    private static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method);

    private static bool HasCsrfHeader(HttpRequest request) =>
        string.Equals(request.Headers[BffDefaults.CsrfHeader], BffDefaults.CsrfHeaderValue, StringComparison.Ordinal);
}
