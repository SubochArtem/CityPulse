using Users.Business.Helpers;
using Users.Tests.Unit.Helpers.TestData;
using Xunit;

namespace Users.Tests.Unit.Helpers.Tests;

public sealed class UriHelperTests
{
    [Theory]
    [MemberData(nameof(UriHelperTestData.ValidUriCases), MemberType = typeof(UriHelperTestData))]
    public void BuildHttpsUri_ValidInputs_ReturnsExpectedAbsoluteUri(string host, string? path, string expectedUri)
    {
        var result = UriHelper.BuildHttpsUri(host, path);

        Assert.Equal(expectedUri, result.AbsoluteUri);
        Assert.Equal(Uri.UriSchemeHttps, result.Scheme);
    }

    [Theory]
    [MemberData(nameof(UriHelperTestData.InvalidHosts), MemberType = typeof(UriHelperTestData))]
    public void BuildHttpsUri_InvalidHostFormat_ThrowsArgumentOrFormatException(string invalidHost)
    {
        Assert.ThrowsAny<Exception>(() => UriHelper.BuildHttpsUri(invalidHost));
    }
}
