using Gateway.API.Constants;

namespace Gateway.API.Bff;

public static class BffReturnUrl
{
    public static string Normalize(string? returnUrl) =>
        IsLocal(returnUrl) ? returnUrl! : BffDefaults.RootPath;

    private static bool IsLocal(string? url)
    {
        if (string.IsNullOrEmpty(url) || url[0] != '/' || url.Any(char.IsControl))
        {
            return false;
        }

        return url.Length == 1 || (url[1] != '/' && url[1] != '\\');
    }
}
