using System;
using Users.Business.Helpers;
using Xunit;

namespace Users.Tests.Unit.Helpers;

public sealed class UriHelperTests
{
    private const string DefaultHost = "tenant.example.com";
    private const string DefaultPath = "api/v2";
    private const string DefaultPathWithLeadingSlash = "/api/v2";
    private const string ExpectedHttpsUrl = "https://tenant.example.com/api/v2";

    [Fact]
    public void BuildHttpsUri_PathIsNotProvided_ReturnsHttpsUriWithRootPath()
    {
        var uri = UriHelper.BuildHttpsUri(DefaultHost);

        Assert.Equal($"https://{DefaultHost}/", uri.ToString());
        Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
        Assert.Equal(DefaultHost, uri.Host);
        Assert.Equal("/", uri.AbsolutePath);
    }

    [Fact]
    public void BuildHttpsUri_PathIsNull_SameResultAsPathOmitted()
    {
        var withDefault = UriHelper.BuildHttpsUri(DefaultHost);
        var withExplicitNull = UriHelper.BuildHttpsUri(DefaultHost, null);

        Assert.Equal(withDefault.ToString(), withExplicitNull.ToString());
    }

    [Fact]
    public void BuildHttpsUri_PathIsProvided_ReturnsHttpsUriWithPath()
    {
        var uri = UriHelper.BuildHttpsUri(DefaultHost, DefaultPath);

        Assert.Equal(ExpectedHttpsUrl, uri.ToString());
        Assert.Equal(DefaultPathWithLeadingSlash, uri.AbsolutePath);
    }

    [Fact]
    public void BuildHttpsUri_PathHasLeadingSlash_DoesNotDuplicateSlash()
    {
        var uri = UriHelper.BuildHttpsUri(DefaultHost, DefaultPathWithLeadingSlash);

        Assert.Equal(ExpectedHttpsUrl, uri.ToString());
    }

    [Fact]
    public void BuildHttpsUri_TwoCallsWithSameHost_ProduceIdenticalStringRepresentation()
    {
        var first = UriHelper.BuildHttpsUri(DefaultHost);
        var second = UriHelper.BuildHttpsUri(DefaultHost);

        Assert.Equal(first.ToString(), second.ToString());
    }

    [Fact]
    public void BuildHttpsUri_HostContainsEmbeddedScheme_ThrowsUriFormatException()
    {
        var hostWithScheme = $"https://{DefaultHost}";

        Assert.Throws<UriFormatException>(() => UriHelper.BuildHttpsUri(hostWithScheme));
    }

    [Theory]
    [MemberData(nameof(ValidHosts))]
    public void BuildHttpsUri_ValidHost_SchemeIsAlwaysHttps(string host)
    {
        var uri = UriHelper.BuildHttpsUri(host);

        Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
    }
    
    public static TheoryData<string> ValidHosts => new()
    {
        DefaultHost,
        "my-tenant.eu.auth0.com",
        "localhost"
    };
}
