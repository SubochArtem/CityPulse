using Feedback.API.Common.Services;
using Xunit;

namespace Feedback.Tests.Unit.Services.Tests;

public sealed class DateTimeProviderTests
{
    private readonly DateTimeProvider _sut = new();

    [Fact]
    public void UtcNow_Always_IsBetweenSurroundingClockReadings()
    {
        var before = DateTimeOffset.UtcNow;

        var result = _sut.UtcNow;

        var after = DateTimeOffset.UtcNow;
        Assert.InRange(result, before, after);
    }

    [Fact]
    public void UtcNow_Always_HasZeroOffset()
    {
        var result = _sut.UtcNow;

        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Fact]
    public void UtcNow_CalledTwice_DoesNotGoBackwards()
    {
        var first = _sut.UtcNow;

        var second = _sut.UtcNow;

        Assert.True(second >= first);
    }
}
