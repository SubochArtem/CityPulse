using System.Security.Claims;
using Gateway.API.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.API.Bff;

public static class BffEndpoints
{
    private const string NoStore = "no-store";

    public static void MapBffEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BffDefaults.LoginPath, Login);
        app.MapGet(BffDefaults.UserPath, (Func<HttpContext, Task<IResult>>)GetUserAsync);
        app.MapGet(BffDefaults.LogoutPath, LogoutAsync);
    }

    private static IResult Login(
        [FromQuery(Name = BffDefaults.ReturnUrlParam)] string? returnUrl,
        [FromQuery(Name = BffDefaults.UiLocalesParam)] string? uiLocales)
    {
        var properties = new AuthenticationProperties { RedirectUri = BffReturnUrl.Normalize(returnUrl) };
        if (!string.IsNullOrWhiteSpace(uiLocales))
        {
            properties.Items[BffDefaults.UiLocalesParam] = uiLocales;
        }

        return Results.Challenge(properties, [BffDefaults.OidcScheme]);
    }

    private static async Task<IResult> GetUserAsync(HttpContext context)
    {
        context.Response.Headers.CacheControl = NoStore;

        var result = await context.AuthenticateAsync(BffDefaults.CookieScheme);
        if (!result.Succeeded)
        {
            return Results.Ok(BffUserResponse.Anonymous);
        }

        var sessionId = result.Principal.FindFirstValue(BffDefaults.SessionIdClaim);
        var accessToken = result.Properties.GetTokenValue(BffDefaults.AccessTokenName);

        return Results.Ok(new BffUserResponse(
            true,
            result.Principal.FindFirstValue(BffDefaults.SubjectClaim),
            AccessTokenReader.ReadPermissions(accessToken),
            sessionId is null ? null : BffRedirects.Logout(sessionId)));
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext context,
        [FromQuery(Name = BffDefaults.SessionIdParam)] string? sessionId)
    {
        var result = await context.AuthenticateAsync(BffDefaults.CookieScheme);
        if (!result.Succeeded)
        {
            return Results.Redirect(BffDefaults.RootPath);
        }

        var expected = result.Principal.FindFirstValue(BffDefaults.SessionIdClaim);
        if (string.IsNullOrEmpty(sessionId) || !string.Equals(sessionId, expected, StringComparison.Ordinal))
        {
            return Results.BadRequest();
        }

        return Results.SignOut(
            new AuthenticationProperties { RedirectUri = BffDefaults.RootPath },
            [BffDefaults.CookieScheme, BffDefaults.OidcScheme]);
    }
}
