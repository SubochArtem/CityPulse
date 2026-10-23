using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Polly.CircuitBreaker;
using Users.Business.Configurations;
using Users.Business.Policies;
using Users.Tests.Infrastructure;
using Users.Tests.Unit.Policies.TestData;
using Xunit;

namespace Users.Tests.Unit.Policies.Tests;

public sealed class HttpPoliciesTests : IDisposable
{
    private const string RequestUrl = "https://tenant.example.com/resource";
    private const int MaxRetryAttempts = 3;
    private const int CircuitBreakerMinimumThroughput = 20;
    private static readonly TimeSpan TimeStep = TimeSpan.FromSeconds(1);

    private readonly FakeTimeProvider _timeProvider = new();
    private readonly Queue<Func<HttpResponseMessage>> _responses = new();
    private readonly object _responsesLock = new();
    private readonly StubHttpMessageHandler _handler;
    private readonly ServiceProvider _serviceProvider;
    private readonly HttpClient _client;
    private Func<HttpResponseMessage> _fallbackResponse = () => new HttpResponseMessage(HttpStatusCode.OK);

    public HttpPoliciesTests()
    {
        _handler = new StubHttpMessageHandler(_ => NextResponse());

        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(_timeProvider);
        services
            .AddHttpClient(IdentityProviderConstants.Auth0HttpClientName)
            .AddResiliencePolicies()
            .ConfigurePrimaryHttpMessageHandler(() => _handler);

        _serviceProvider = services.BuildServiceProvider();
        _client = _serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(IdentityProviderConstants.Auth0HttpClientName);
    }

    public void Dispose()
    {
        _client.Dispose();
        _serviceProvider.Dispose();
    }

    private HttpResponseMessage NextResponse()
    {
        Func<HttpResponseMessage> response;

        lock (_responsesLock)
        {
            response = _responses.Count > 0 ? _responses.Dequeue() : _fallbackResponse;
        }

        return response();
    }

    private void EnqueueStatus(HttpStatusCode statusCode) =>
        _responses.Enqueue(() => new HttpResponseMessage(statusCode));

    private void EnqueueException() =>
        _responses.Enqueue(() => throw new HttpRequestException());

    private void AlwaysRespondWith(HttpStatusCode statusCode) =>
        _fallbackResponse = () => new HttpResponseMessage(statusCode);

    private async Task<T> CompleteAsync<T>(Task<T> task)
    {
        while (!task.IsCompleted)
        {
            _timeProvider.Advance(TimeStep);
            await Task.WhenAny(task, Task.Delay(1));
        }

        return await task;
    }

    private async Task WaitForRequestsAsync(int expectedCount)
    {
        while (_handler.Requests.Count < expectedCount)
            await Task.Delay(1);
    }

    [Fact]
    public async Task SendAsync_SuccessfulResponse_DoesNotRetry()
    {
        var response = await CompleteAsync(_client.GetAsync(RequestUrl));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(_handler.Requests);
    }

    [Theory]
    [MemberData(nameof(HttpPoliciesTestData.RetryableStatusCodes), MemberType = typeof(HttpPoliciesTestData))]
    public async Task SendAsync_FailureStatusThenSuccess_RetriesAndReturnsSuccess(HttpStatusCode failureStatus)
    {
        EnqueueStatus(failureStatus);
        EnqueueStatus(HttpStatusCode.OK);

        var response = await CompleteAsync(_client.GetAsync(RequestUrl));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, _handler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_HttpRequestExceptionThenSuccess_RetriesAndReturnsSuccess()
    {
        EnqueueException();
        EnqueueStatus(HttpStatusCode.OK);

        var response = await CompleteAsync(_client.GetAsync(RequestUrl));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, _handler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_PersistentFailure_StopsAfterMaxRetriesAndReturnsLastFailure()
    {
        AlwaysRespondWith(HttpStatusCode.ServiceUnavailable);

        var response = await CompleteAsync(_client.GetAsync(RequestUrl));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(MaxRetryAttempts + 1, _handler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_ManyConcurrentFailures_OpensCircuitAndRejectsFurtherAttempts()
    {
        AlwaysRespondWith(HttpStatusCode.InternalServerError);

        var calls = Enumerable
            .Range(0, CircuitBreakerMinimumThroughput)
            .Select(_ => _client.GetAsync(RequestUrl))
            .ToList();

        await WaitForRequestsAsync(CircuitBreakerMinimumThroughput);

        foreach (var call in calls)
            await Assert.ThrowsAsync<BrokenCircuitException>(() => CompleteAsync(call));

        Assert.Equal(CircuitBreakerMinimumThroughput, _handler.Requests.Count);
        await Assert.ThrowsAsync<BrokenCircuitException>(() => CompleteAsync(_client.GetAsync(RequestUrl)));
        Assert.Equal(CircuitBreakerMinimumThroughput, _handler.Requests.Count);
    }
}
