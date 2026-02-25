namespace Users.Business.Helpers;

public static class UriHelper
{
    public static Uri BuildHttpsUri(string host, string? path = null)
    {
        var builder = new UriBuilder
        {
            Scheme = Uri.UriSchemeHttps,
            Host = host,
            Path = path
        };
        return builder.Uri;
    }
}
