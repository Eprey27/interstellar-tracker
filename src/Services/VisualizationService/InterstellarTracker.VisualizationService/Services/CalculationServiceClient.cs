using InterstellarTracker.VisualizationService.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InterstellarTracker.VisualizationService.Services;

/// <summary>
/// HTTP client for communicating with the CalculationService microservice
/// 
/// TDD GREEN PHASE - MINIMAL IMPLEMENTATION
/// This implementation provides the simplest code to make tests pass.
/// No Polly policies, no caching, no advanced error handling yet.
/// </summary>
public class CalculationServiceClient : ICalculationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CalculationServiceClient> _logger;

    public CalculationServiceClient(
        HttpClient httpClient,
        ILogger<CalculationServiceClient> logger,
        int cacheTtlSeconds = 3600)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get trajectory data from CalculationService
    /// GREEN PHASE: Minimal implementation with basic HTTP POST
    /// </summary>
    public async Task<List<TrajectoryPoint>?> GetTrajectoryAsync(
        string objectId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Requesting trajectory for {ObjectId} from {StartDate} to {EndDate}",
            objectId, startDate, endDate);

        try
        {
            var request = new TrajectoryRequest
            {
                ObjectId = objectId,
                StartDate = startDate,
                EndDate = endDate,
                IntervalHours = 6
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/calculations/trajectory",
                request,
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Object {ObjectId} not found in CalculationService", objectId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var trajectoryResponse = await response.Content.ReadFromJsonAsync<TrajectoryResponseDto>(
                cancellationToken: cancellationToken);

            return trajectoryResponse?.Points ?? new List<TrajectoryPoint>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling CalculationService for {ObjectId}", objectId);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling CalculationService for {ObjectId}", objectId);
            throw;
        }
    }

    /// <summary>
    /// Get position at specific time from CalculationService
    /// GREEN PHASE: Minimal implementation with basic HTTP GET
    /// </summary>
    public async Task<(Vector3D Position, Vector3D Velocity, double Distance)?> GetPositionAsync(
        string objectId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Requesting position for {ObjectId} at {Date}",
            objectId, date);

        try
        {
            var encodedObjectId = Uri.EscapeDataString(objectId);
            var dateParam = date.ToString("o"); // ISO 8601

            var response = await _httpClient.GetAsync(
                $"/api/calculations/position/{encodedObjectId}?date={dateParam}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Position for {ObjectId} at {Date} not found", objectId, date);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var positionResponse = await response.Content.ReadFromJsonAsync<PositionResponseDto>(
                cancellationToken: cancellationToken);

            if (positionResponse == null)
            {
                return null;
            }

            return (positionResponse.Position, positionResponse.Velocity, positionResponse.DistanceFromSun);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling CalculationService for {ObjectId}", objectId);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling CalculationService for {ObjectId}", objectId);
            throw;
        }
    }

    #region DTOs for CalculationService API

    private record TrajectoryRequest
    {
        public required string ObjectId { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public int IntervalHours { get; init; }
    }

    private record TrajectoryResponseDto
    {
        public required string ObjectId { get; init; }
        public required List<TrajectoryPoint> Points { get; init; }
        public DateTime GeneratedAt { get; init; }
    }

    private record PositionResponseDto
    {
        public required string ObjectId { get; init; }
        public DateTime Timestamp { get; init; }
        public required Vector3D Position { get; init; }
        public required Vector3D Velocity { get; init; }
        public double DistanceFromSun { get; init; }
    }

    #endregion
}
