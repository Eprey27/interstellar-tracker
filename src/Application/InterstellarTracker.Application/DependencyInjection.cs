using System.Reflection;
using FluentValidation;
using InterstellarTracker.Application.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterstellarTracker.Application;

/// <summary>
/// Dependency injection configuration for the Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container.
    /// Registers CQRS handlers, validators, and configuration services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR (CQRS pattern)
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register configuration services (RSPEC-1075: externalize URIs and paths)
        services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();

        return services;
    }
}
