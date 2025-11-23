using Microsoft.Extensions.DependencyInjection;

namespace InterstellarTracker.CalculationService.Extensions;

/// <summary>
/// Extension methods for configuring services in the Calculation Service.
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
    public static IServiceCollection AddCalculationServiceTelemetry(
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
    /// Avoids permissive AllowAnyOrigin() in production.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCalculationServiceCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("CalculationServicePolicy", policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    // Fallback: allow only localhost for development
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5000")
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
    /// Maps middleware for API services.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseCalculationServiceMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors("CalculationServicePolicy");
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        return app;
    }
}
