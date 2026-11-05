using Gateway.API.Configuration;
using Gateway.API.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Gateway.API.Bff;

public sealed class DistributedTicketStore(
    IDistributedCache cache,
    IDataProtectionProvider dataProtectionProvider,
    IOptions<BffSettings> settings,
    TimeProvider timeProvider) : ITicketStore
{
    private const string ProtectorPurpose = "Gateway.Bff.Session";

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = BffDefaults.SessionKeyPrefix + Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket);

        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var expiresAt = ticket.Properties.ExpiresUtc
            ?? timeProvider.GetUtcNow().AddDays(settings.Value.SessionLifetimeDays);
        var payload = _protector.Protect(TicketSerializer.Default.Serialize(ticket));

        return cache.SetAsync(key, payload, new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAt });
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var payload = await cache.GetAsync(key);
        if (payload is null)
        {
            return null;
        }

        try
        {
            return TicketSerializer.Default.Deserialize(_protector.Unprotect(payload));
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }
    }

    public Task RemoveAsync(string key) => cache.RemoveAsync(key);
}
