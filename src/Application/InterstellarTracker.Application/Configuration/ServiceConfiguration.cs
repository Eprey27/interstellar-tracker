using Microsoft.Extensions.Configuration;

namespace InterstellarTracker.Application.Configuration;

/// <summary>
/// Implementation of <see cref="IServiceConfiguration"/> that loads service URLs from configuration.
/// Supports both appsettings.json and environment variables following ASP.NET Core conventions.
/// </summary>
public sealed class ServiceConfiguration : IServiceConfiguration
{
    private readonly IConfiguration _configuration;
    private readonly string _defaultBaseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public ServiceConfiguration(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _defaultBaseUrl = "http://localhost";
    }

    /// <inheritdoc />
    public Uri GetCalculationServiceUrl()
    {
        var url = _configuration["Services:CalculationService:Url"]
                ?? _configuration["SERVICES_CALCULATIONSERVICE_URL"]
                ?? $"{_defaultBaseUrl}:5001";

        return ValidateAndCreateUri(url, "CalculationService");
    }

    /// <inheritdoc />
    public Uri GetVisualizationServiceUrl()
    {
        var url = _configuration["Services:VisualizationService:Url"]
                ?? _configuration["SERVICES_VISUALIZATIONSERVICE_URL"]
                ?? $"{_defaultBaseUrl}:5002";

        return ValidateAndCreateUri(url, "VisualizationService");
    }

    /// <inheritdoc />
    public Uri GetAuthServiceUrl()
    {
        var url = _configuration["Services:AuthService:Url"]
                ?? _configuration["SERVICES_AUTHSERVICE_URL"]
                ?? $"{_defaultBaseUrl}:5003";

        return ValidateAndCreateUri(url, "AuthService");
    }

    /// <inheritdoc />
    public Uri GetApiGatewayUrl()
    {
        var url = _configuration["Services:ApiGateway:Url"]
                ?? _configuration["SERVICES_APIGATEWAY_URL"]
                ?? $"{_defaultBaseUrl}:5000";

        return ValidateAndCreateUri(url, "ApiGateway");
    }

    /// <summary>
    /// Validates and creates a URI from a configuration value.
    /// </summary>
    /// <param name="urlValue">The URL string from configuration.</param>
    /// <param name="serviceName">The name of the service (for diagnostic purposes).</param>
    /// <returns>A validated <see cref="Uri"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the URL is invalid or empty.</exception>
    private static Uri ValidateAndCreateUri(string urlValue, string serviceName)
    {
        if (string.IsNullOrWhiteSpace(urlValue))
        {
            throw new InvalidOperationException(
                $"Configuration for {serviceName} URL is missing. " +
                $"Ensure 'Services:{serviceName}:Url' is set in appsettings.json or " +
                $"'SERVICES_{serviceName.ToUpper()}_URL' environment variable is configured.");
        }

        if (!Uri.TryCreate(urlValue, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException(
                $"Configuration for {serviceName} URL is invalid: '{urlValue}'. " +
                $"Must be a valid absolute URI (e.g., 'https://api.example.com').");
        }

        return uri;
    }
}
