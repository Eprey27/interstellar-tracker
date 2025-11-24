namespace InterstellarTracker.Application.Configuration;

/// <summary>
/// Defines the contract for service configuration providing externalized URLs and endpoints.
/// Implements RSPEC-1075: Avoid hardcoding URIs and absolute paths.
/// </summary>
public interface IServiceConfiguration
{
    /// <summary>
    /// Gets the base URL for the Calculation Service.
    /// </summary>
    /// <returns>The fully qualified URL (e.g., "http://localhost:5001" or "https://api.example.com/calculation")</returns>
    Uri GetCalculationServiceUrl();

    /// <summary>
    /// Gets the base URL for the Visualization Service.
    /// </summary>
    /// <returns>The fully qualified URL for the Visualization Service</returns>
    Uri GetVisualizationServiceUrl();

    /// <summary>
    /// Gets the base URL for the Authentication Service.
    /// </summary>
    /// <returns>The fully qualified URL for the Authentication Service</returns>
    Uri GetAuthServiceUrl();

    /// <summary>
    /// Gets the base URL for the API Gateway.
    /// </summary>
    /// <returns>The fully qualified URL for the API Gateway</returns>
    Uri GetApiGatewayUrl();
}
