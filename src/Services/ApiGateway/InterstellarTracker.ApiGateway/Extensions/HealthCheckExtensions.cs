using InterstellarTracker.Application.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterstellarTracker.ApiGateway.Extensions;

/// <summary>
/// Extension methods for configuring health checks in the API Gateway.
/// Implements RSPEC-1075: externalize service URLs and endpoints.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Configures health checks for downstream microservices using externalized configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceConfig">The service configuration provider.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddDownstreamServiceHealthChecks(
        this IServiceCollection services,
        IServiceConfiguration serviceConfig)
    {
        var calculationServiceUrl = serviceConfig.GetCalculationServiceUrl();
        var visualizationServiceUrl = serviceConfig.GetVisualizationServiceUrl();

        return services.AddHealthChecks()
            .AddUrlGroup(
                new Uri(calculationServiceUrl, "/health"),
                name: "calculation-service",
                timeout: TimeSpan.FromSeconds(5),
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy)
            .AddUrlGroup(
                new Uri(visualizationServiceUrl, "/health"),
                name: "visualization-service",
                timeout: TimeSpan.FromSeconds(5),
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);
    }
}
