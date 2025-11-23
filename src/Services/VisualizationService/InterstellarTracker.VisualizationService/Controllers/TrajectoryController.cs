using InterstellarTracker.VisualizationService.Models;
using InterstellarTracker.VisualizationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InterstellarTracker.VisualizationService.Controllers;

/// <summary>
/// Controller for trajectory and position data endpoints
/// </summary>
[ApiController]
[Route("api")]
public class TrajectoryController : ControllerBase
{
    private readonly ILogger<TrajectoryController> _logger;
    private readonly ITrajectoryService _trajectoryService;

    public TrajectoryController(
        ILogger<TrajectoryController> logger,
        ITrajectoryService trajectoryService)
    {
        _logger = logger;
        _trajectoryService = trajectoryService;
    }

    /// <summary>
    /// Get trajectory data for an interstellar object
    /// </summary>
    /// <param name="objectId">Object identifier (e.g., "2I/Borisov")</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Trajectory with list of points</returns>
    [HttpGet("trajectories/{objectId}")]
    [ProducesResponseType<TrajectoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrajectoryResponse>> GetTrajectory(
        string objectId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // URL decode the objectId (e.g., "2I%2FBorisov" -> "2I/Borisov")
        objectId = Uri.UnescapeDataString(objectId);

        _logger.LogInformation("Getting trajectory for object {ObjectId} from {StartDate} to {EndDate}",
            objectId, startDate, endDate);

        var response = await _trajectoryService.GetTrajectoryAsync(
            objectId, startDate, endDate, cancellationToken);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    /// <summary>
    /// Get position of object at specific date
    /// </summary>
    /// <param name="objectId">Object identifier</param>
    /// <param name="date">Date for position calculation</param>
    /// <returns>Position and velocity at specified date</returns>
    [HttpGet("positions/{objectId}")]
    [ProducesResponseType<PositionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PositionResponse>> GetPosition(
        string objectId,
        [FromQuery] DateTime date,
        CancellationToken cancellationToken = default)
    {
        // URL decode the objectId
        objectId = Uri.UnescapeDataString(objectId);

        _logger.LogInformation("Getting position for object {ObjectId} at {Date}",
            objectId, date);

        var response = await _trajectoryService.GetPositionAsync(
            objectId, date, cancellationToken);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }
}
