using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace InterstellarTracker.WebUI.Extensions;

/// <summary>
/// Extension methods for configuring services in the Web UI.
/// Provides centralized configuration management following the Composition Root pattern.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Configures Application Insights telemetry with best practices.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWebUITelemetry(
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
    /// Configures middleware for the Web UI application.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseWebUIMiddleware(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseHttpMetrics();
        app.UseStaticFiles();
        app.UseAntiforgery();

        return app;
    }
}
