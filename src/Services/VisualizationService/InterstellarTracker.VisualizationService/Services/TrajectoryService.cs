using InterstellarTracker.VisualizationService.Models;

namespace InterstellarTracker.VisualizationService.Services;

/// <summary>
/// Implementation of trajectory service using CalculationService HTTP client
/// Refactored from mock data to real microservice integration (TDD REFACTOR phase)
/// </summary>
public class TrajectoryService : ITrajectoryService
{
    private readonly ILogger<TrajectoryService> _logger;
    private readonly ICalculationServiceClient _calculationServiceClient;

    public TrajectoryService(
        ILogger<TrajectoryService> logger,
        ICalculationServiceClient calculationServiceClient)
    {
        _logger = logger;
        _calculationServiceClient = calculationServiceClient;
    }

    /// <inheritdoc />
    public async Task<TrajectoryResponse?> GetTrajectoryAsync(
        string objectId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Calculating trajectory for {ObjectId} from {StartDate} to {EndDate}",
            objectId, startDate, endDate);

        // Call CalculationService to get trajectory data
        var points = await _calculationServiceClient.GetTrajectoryAsync(
            objectId,
            startDate,
            endDate,
            cancellationToken);

        if (points == null)
        {
            _logger.LogWarning("Object {ObjectId} not found in CalculationService", objectId);
            return null;
        }

        var response = new TrajectoryResponse(
            ObjectId: objectId,
            Points: points,
            GeneratedAt: DateTime.UtcNow
        );

        _logger.LogInformation(
            "Retrieved trajectory for {ObjectId} with {PointCount} points",
            objectId, points.Count);

        return response;
    }

    /// <inheritdoc />
    public async Task<PositionResponse?> GetPositionAsync(
        string objectId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Calculating position for {ObjectId} at {Date}",
            objectId, date);

        // Call CalculationService to get position at specific time
        var result = await _calculationServiceClient.GetPositionAsync(
            objectId,
            date,
            cancellationToken);

        if (result == null)
        {
            _logger.LogWarning("Position for {ObjectId} at {Date} not found in CalculationService", objectId, date);
            return null;
        }

        var (position, velocity, distance) = result.Value;

        var response = new PositionResponse(
            ObjectId: objectId,
            Timestamp: date,
            Position: position,
            Velocity: velocity,
            DistanceFromSun: distance
        );

        _logger.LogInformation(
            "Retrieved position for {ObjectId} at {Date}",
            objectId, date);

        return response;
    }
}
