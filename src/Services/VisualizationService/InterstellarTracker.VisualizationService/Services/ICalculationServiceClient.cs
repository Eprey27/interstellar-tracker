using InterstellarTracker.VisualizationService.Models;

namespace InterstellarTracker.VisualizationService.Services;

/// <summary>
/// Client interface for communicating with the CalculationService microservice
/// to retrieve orbital calculations for interstellar objects.
/// </summary>
public interface ICalculationServiceClient
{
    /// <summary>
    /// Get trajectory data from CalculationService for an interstellar object
    /// </summary>
    /// <param name="objectId">Interstellar object identifier (e.g., "2I/Borisov")</param>
    /// <param name="startDate">Optional start date for trajectory calculation (defaults to 6 months before perihelion)</param>
    /// <param name="endDate">Optional end date for trajectory calculation (defaults to 6 months after perihelion)</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>
    /// List of trajectory points with position/velocity data, or null if object not found.
    /// Each point represents the object's state at a specific timestamp.
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when CalculationService is unavailable or returns an error</exception>
    /// <exception cref="TimeoutException">Thrown when the request exceeds the configured timeout</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when response data cannot be deserialized</exception>
    Task<List<TrajectoryPoint>?> GetTrajectoryAsync(
        string objectId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get position and velocity at a specific time from CalculationService
    /// </summary>
    /// <param name="objectId">Interstellar object identifier (e.g., "2I/Borisov")</param>
    /// <param name="date">Specific date/time for position calculation</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>
    /// Tuple containing Position (AU), Velocity (AU/day), and Distance from Sun (AU),
    /// or null if object not found at the specified date.
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when CalculationService is unavailable or returns an error</exception>
    /// <exception cref="TimeoutException">Thrown when the request exceeds the configured timeout</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when response data cannot be deserialized</exception>
    Task<(Vector3D Position, Vector3D Velocity, double Distance)?> GetPositionAsync(
        string objectId,
        DateTime date,
        CancellationToken cancellationToken = default);
}
