using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Users.Business.Policies;

public static class HttpPolicies
{
    private const string ResiliencePipelineName = "default";

    public static IHttpClientBuilder AddResiliencePolicies(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler(ResiliencePipelineName, pipeline =>
        {
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: HttpRequestException } => PredicateResult.True(),
                    { Result.StatusCode: HttpStatusCode.TooManyRequests } => PredicateResult.True(),
                    { Result.IsSuccessStatusCode: false } => PredicateResult.True(),
                    _ => PredicateResult.False()
                }
            });

            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 20,
                BreakDuration = TimeSpan.FromSeconds(30)
            });
        });

        return builder;
    }
}
