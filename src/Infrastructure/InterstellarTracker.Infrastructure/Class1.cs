using InterstellarTracker.Application.Common.Interfaces;
using InterstellarTracker.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace InterstellarTracker.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure layer services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register in-memory repository as singleton
        // In a production system, this would be replaced with a database-backed repository
        services.AddSingleton<ICelestialBodyRepository, InMemoryCelestialBodyRepository>();

        return services;
    }
}
