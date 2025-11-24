using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace InterstellarTracker.ApiGateway.Extensions;

/// <summary>
/// Extension methods for configuring middleware in the API Gateway.
/// Implements secure defaults and separation of concerns.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Configures Application Insights telemetry with best practices.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGatewayTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
            options.EnableAdaptiveSampling = true;
            options.EnableQuickPulseMetricStream = true;
        });

        return services;
    }

    /// <summary>
    /// Configures CORS policy with externalized allowed origins.
    /// Supports development and production configurations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGatewayCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("GatewayPolicy", policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    // Fallback: allow only localhost for development
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
                else
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Maps security and monitoring middleware.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseGatewayMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("GatewayPolicy");
        }

        app.UseHttpsRedirection();
        app.UseHttpMetrics();

        return app;
    }
}
