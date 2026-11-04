using System.Text;
using Feedback.API.Features.Activity.GetActivity;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace Feedback.Tests.Unit.Activity.Tests;

public sealed class ActivityCursorTests
{
    private static string EncodeRaw(string raw) => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(raw));

    [Fact]
    public void Encode_ThenDecode_ReturnsSameCursorAtMillisecondPrecision()
    {
        var cursor = new ActivityCursor(new DateTimeOffset(2026, 11, 5, 12, 30, 15, 123, TimeSpan.Zero), Guid.NewGuid());

        var decoded = ActivityCursor.TryDecode(cursor.Encode(), out var result);

        Assert.True(decoded);
        Assert.Equal(cursor, result);
    }

    [Fact]
    public void Encode_Always_ProducesUrlSafeValue()
    {
        var encoded = new ActivityCursor(DateTimeOffset.UnixEpoch.AddYears(56), Guid.NewGuid()).Encode();

        Assert.DoesNotContain('+', encoded);
        Assert.DoesNotContain('/', encoded);
        Assert.DoesNotContain('=', encoded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not base64 !")]
    public void TryDecode_MissingOrMalformed_ReturnsFalse(string? value)
    {
        Assert.False(ActivityCursor.TryDecode(value, out _));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc_0123456789abcdef0123456789abcdef")]
    [InlineData("-5_0123456789abcdef0123456789abcdef")]
    [InlineData("123_not-a-guid")]
    [InlineData("123_0123456789abcdef0123456789abcdef_extra")]
    [InlineData("99999999999999999_0123456789abcdef0123456789abcdef")]
    public void TryDecode_WrongPayload_ReturnsFalse(string raw)
    {
        Assert.False(ActivityCursor.TryDecode(EncodeRaw(raw), out _));
    }
}
