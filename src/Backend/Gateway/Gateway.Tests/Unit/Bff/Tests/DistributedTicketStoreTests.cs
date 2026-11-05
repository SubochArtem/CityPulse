using System.Security.Claims;
using System.Text;
using Gateway.API.Bff;
using Gateway.API.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gateway.Tests.Unit.Bff.Tests;

public sealed class DistributedTicketStoreTests
{
    private const string Scheme = "BffCookie";
    private const string UserId = "auth0|user";
    private const string AccessToken = "secret-access-token-value";

    private readonly MemoryDistributedCache _cache = new(Options.Create(new MemoryDistributedCacheOptions()));
    private readonly DistributedTicketStore _store;

    public DistributedTicketStoreTests()
    {
        _store = new DistributedTicketStore(
            _cache,
            new EphemeralDataProtectionProvider(),
            Options.Create(new BffSettings()),
            TimeProvider.System);
    }

    [Fact]
    public async Task StoreAndRetrieve_ShouldRoundTripPrincipalAndTokens()
    {
        var key = await _store.StoreAsync(CreateTicket());

        var ticket = await _store.RetrieveAsync(key);

        Assert.Equal(UserId, ticket!.Principal.FindFirstValue("sub"));
        Assert.Equal(AccessToken, ticket.Properties.GetTokenValue("access_token"));
    }

    [Fact]
    public async Task Store_ShouldNotKeepTokensInPlainText()
    {
        var key = await _store.StoreAsync(CreateTicket());

        var payload = await _cache.GetAsync(key);

        Assert.DoesNotContain(AccessToken, Encoding.UTF8.GetString(payload!));
    }

    [Fact]
    public async Task Remove_ShouldForgetSession()
    {
        var key = await _store.StoreAsync(CreateTicket());

        await _store.RemoveAsync(key);

        Assert.Null(await _store.RetrieveAsync(key));
    }

    [Fact]
    public async Task Retrieve_TamperedPayload_ShouldReturnNull()
    {
        var key = await _store.StoreAsync(CreateTicket());
        await _cache.SetAsync(key, Encoding.UTF8.GetBytes("tampered"), new DistributedCacheEntryOptions());

        Assert.Null(await _store.RetrieveAsync(key));
    }

    [Fact]
    public async Task Retrieve_UnknownKey_ShouldReturnNull()
    {
        Assert.Null(await _store.RetrieveAsync("bff:session:missing"));
    }

    private static AuthenticationTicket CreateTicket()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", UserId)], Scheme));
        var properties = new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) };
        properties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = AccessToken }]);

        return new AuthenticationTicket(principal, properties, Scheme);
    }
}
