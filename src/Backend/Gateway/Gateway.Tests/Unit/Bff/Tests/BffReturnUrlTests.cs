using Gateway.API.Bff;
using Xunit;

namespace Gateway.Tests.Unit.Bff.Tests;

public sealed class BffReturnUrlTests
{
    private const string Root = "/";

    [Theory]
    [InlineData("/")]
    [InlineData("/polls/42")]
    [InlineData("/cities?page=2&q=grodno")]
    public void Normalize_LocalPath_ShouldKeepIt(string returnUrl)
    {
        Assert.Equal(returnUrl, BffReturnUrl.Normalize(returnUrl));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://evil.example/")]
    [InlineData("//evil.example")]
    [InlineData("/\\evil.example")]
    [InlineData("polls/42")]
    [InlineData("/polls\r\nSet-Cookie: x=1")]
    public void Normalize_ForeignOrMalformedUrl_ShouldFallBackToRoot(string? returnUrl)
    {
        Assert.Equal(Root, BffReturnUrl.Normalize(returnUrl));
    }
}
