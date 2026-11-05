using Gateway.API.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Gateway.API.Bff;

public sealed class SessionKeyReader(IOptionsMonitor<CookieAuthenticationOptions> cookieOptions)
{
    public string? Read(HttpContext context)
    {
        var options = cookieOptions.Get(BffDefaults.CookieScheme);
        var cookie = options.CookieManager.GetRequestCookie(context, options.Cookie.Name!);
        if (string.IsNullOrEmpty(cookie))
        {
            return null;
        }

        return options.TicketDataFormat.Unprotect(cookie)?.Principal
            .FindFirst(BffDefaults.CookieSessionKeyClaim)?.Value;
    }
}
