using InterstellarTracker.VisualizationService.Models;

namespace InterstellarTracker.VisualizationService.Services;

/// <summary>
/// Service for calculating and processing trajectory data for interstellar objects
/// </summary>
public interface ITrajectoryService
{
    /// <summary>
    /// Get trajectory points for an interstellar object
    /// </summary>
    /// <param name="objectId">Object identifier (e.g., "2I/Borisov")</param>
    /// <param name="startDate">Optional start date for trajectory filtering</param>
    /// <param name="endDate">Optional end date for trajectory filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trajectory response with list of points, or null if object not found</returns>
    Task<TrajectoryResponse?> GetTrajectoryAsync(
        string objectId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get position and velocity of object at specific date
    /// </summary>
    /// <param name="objectId">Object identifier</param>
    /// <param name="date">Date for position calculation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Position response with coordinates and velocity, or null if object not found</returns>
    Task<PositionResponse?> GetPositionAsync(
        string objectId,
        DateTime date,
        CancellationToken cancellationToken = default);
}
