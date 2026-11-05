using Gateway.API.Constants;
using Microsoft.AspNetCore.WebUtilities;

namespace Gateway.API.Bff;

public static class BffRedirects
{
    public static string LoginFailed(string? description)
    {
        var query = new Dictionary<string, string?> { [BffDefaults.ErrorParam] = BffDefaults.LoginFailedError };
        if (!string.IsNullOrWhiteSpace(description))
        {
            query[BffDefaults.ErrorDescriptionParam] = description;
        }

        return QueryHelpers.AddQueryString(BffDefaults.RootPath, query);
    }

    public static string Logout(string sessionId) =>
        QueryHelpers.AddQueryString(BffDefaults.LogoutPath, BffDefaults.SessionIdParam, sessionId);

    public static string Auth0Logout(string domain, string clientId, string returnTo) =>
        QueryHelpers.AddQueryString(
            BffDefaults.LogoutEndpoint(domain),
            new Dictionary<string, string?>
            {
                [BffDefaults.ClientIdParam] = clientId,
                [BffDefaults.ReturnToParam] = returnTo
            });
}
