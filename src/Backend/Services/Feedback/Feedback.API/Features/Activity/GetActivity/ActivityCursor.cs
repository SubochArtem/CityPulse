using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Feedback.API.Features.Activity.GetActivity;

public readonly record struct ActivityCursor(DateTimeOffset OccurredAt, Guid Id)
{
    private const char Separator = '_';

    public string Encode()
    {
        var raw = string.Create(
            CultureInfo.InvariantCulture,
            $"{OccurredAt.ToUnixTimeMilliseconds()}{Separator}{Id:N}");

        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(raw));
    }

    public static bool TryDecode(string? value, out ActivityCursor cursor)
    {
        cursor = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string raw;
        try
        {
            raw = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(value));
        }
        catch (FormatException)
        {
            return false;
        }

        var parts = raw.Split(Separator);
        if (parts.Length != 2
            || !long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var milliseconds)
            || !Guid.TryParseExact(parts[1], "N", out var id))
            return false;

        try
        {
            cursor = new ActivityCursor(DateTimeOffset.FromUnixTimeMilliseconds(milliseconds), id);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        return true;
    }
}
