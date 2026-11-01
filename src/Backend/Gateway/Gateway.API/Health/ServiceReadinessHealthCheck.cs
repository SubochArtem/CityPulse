using System.Text.Json;
using Gateway.API.Constants;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Yarp.ReverseProxy.Configuration;

namespace Gateway.API.Health;

public sealed class ServiceReadinessHealthCheck(
    IProxyConfigProvider proxyConfigProvider,
    IHttpClientFactory httpClientFactory,
    string clusterId) : IHealthCheck
{
    private const string StatusProperty = "status";
    private const string NotConfiguredMessage = "The service is not configured in the gateway.";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var cluster = proxyConfigProvider.GetConfig().Clusters
            .FirstOrDefault(candidate => string.Equals(candidate.ClusterId, clusterId, StringComparison.OrdinalIgnoreCase));

        if (cluster?.Destinations is not { Count: > 0 } destinations)
        {
            return HealthCheckResult.Unhealthy(NotConfiguredMessage);
        }

        var statuses = await Task.WhenAll(
            destinations.Values.Select(destination => CheckDestinationAsync(destination.Address, cancellationToken)));

        return new HealthCheckResult(statuses.Min());
    }

    private async Task<HealthStatus> CheckDestinationAsync(string address, CancellationToken cancellationToken)
    {
        try
        {
            using var client = httpClientFactory.CreateClient(GatewayDefaults.ServiceHealthClientName);
            using var response = await client.GetAsync(
                new Uri(new Uri(address), GatewayDefaults.ServiceReadyPath),
                cancellationToken);

            return response.IsSuccessStatusCode
                ? await ReadStatusAsync(response, cancellationToken)
                : HealthStatus.Unhealthy;
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthStatus.Unhealthy;
        }
    }

    private static async Task<HealthStatus> ReadStatusAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return document.RootElement.TryGetProperty(StatusProperty, out var status)
                   && Enum.TryParse<HealthStatus>(status.GetString(), ignoreCase: true, out var parsed)
                ? parsed
                : HealthStatus.Healthy;
        }
        catch (JsonException)
        {
            return HealthStatus.Healthy;
        }
    }
}
