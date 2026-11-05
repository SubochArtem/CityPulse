using Gateway.API.Bff;
using Gateway.Tests.Infrastructure;
using Xunit;

namespace Gateway.Tests.Unit.Bff.Tests;

public sealed class AccessTokenReaderTests
{
    private const string PollsRead = "polls.read";
    private const string IdeasCreate = "ideas.create.city";

    [Fact]
    public void ReadPermissions_TokenWithPermissions_ShouldReturnThemOnce()
    {
        var token = BffTestSignInStartupFilter.CreateAccessToken(PollsRead, IdeasCreate, PollsRead);

        var permissions = AccessTokenReader.ReadPermissions(token);

        Assert.Equal([PollsRead, IdeasCreate], permissions);
    }

    [Fact]
    public void ReadPermissions_TokenWithoutPermissions_ShouldReturnEmpty()
    {
        var token = BffTestSignInStartupFilter.CreateAccessToken();

        Assert.Empty(AccessTokenReader.ReadPermissions(token));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    public void ReadPermissions_MissingOrMalformedToken_ShouldReturnEmpty(string? token)
    {
        Assert.Empty(AccessTokenReader.ReadPermissions(token));
    }
}
