using System.Diagnostics;
using System.Net;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Infrastructure.Constants;
using Xunit;

namespace Gateway.Tests.Integration.RateLimiting.Tests;

public sealed class RateLimitingTests(GatewayWebAppFactory factory) : GatewayApiTestBase(factory)
{
    private const string Path = "/api/v1/polls";
    private const int TokenLimit = 3;
    private const int SlowReplenishmentSeconds = 3600;
    private const int FastReplenishmentSeconds = 1;
    private const int MinimumQueueWaitMilliseconds = 500;

    private static readonly IReadOnlyDictionary<string, string?> SmallBucket = new Dictionary<string, string?>
    {
        [TestConstants.ConfigurationKeys.TokenLimit] = TokenLimit.ToString(),
        [TestConstants.ConfigurationKeys.TokensPerPeriod] = "1",
        [TestConstants.ConfigurationKeys.ReplenishmentPeriodSeconds] = SlowReplenishmentSeconds.ToString(),
        [TestConstants.ConfigurationKeys.QueueLimit] = "0"
    };

    [Fact]
    public async Task Send_WithinTokenLimit_ShouldSucceed()
    {
        using var client = CreateClient(SmallBucket);

        for (var i = 0; i < TokenLimit; i++)
        {
            var response = await client.GetAsync(Path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task Send_ExceedingTokenLimit_ShouldReturnTooManyRequests()
    {
        using var client = CreateClient(SmallBucket);
        await SendManyAsync(client, TokenLimit);

        var response = await client.GetAsync(Path);

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task Send_ExceedingTokenLimit_ShouldKeepCorsHeaders()
    {
        using var client = CreateClient(SmallBucket);
        await SendManyAsync(client, TokenLimit);
        using var request = CreateRequest(HttpMethod.Get, Path, TestConstants.Origins.Allowed);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal(TestConstants.Origins.Allowed, GetAllowOrigin(response));
    }

    [Fact]
    public async Task Send_ExceedingTokenLimitWithQueue_ShouldWaitForReplenishment()
    {
        using var client = CreateClient(new Dictionary<string, string?>
        {
            [TestConstants.ConfigurationKeys.TokenLimit] = "1",
            [TestConstants.ConfigurationKeys.TokensPerPeriod] = "1",
            [TestConstants.ConfigurationKeys.ReplenishmentPeriodSeconds] = FastReplenishmentSeconds.ToString(),
            [TestConstants.ConfigurationKeys.QueueLimit] = "1"
        });
        await client.GetAsync(Path);

        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync(Path);
        stopwatch.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds >= MinimumQueueWaitMilliseconds);
    }

    private static async Task SendManyAsync(HttpClient client, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await client.GetAsync(Path);
        }
    }
}
