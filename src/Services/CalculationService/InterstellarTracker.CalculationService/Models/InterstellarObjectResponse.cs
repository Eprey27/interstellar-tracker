namespace InterstellarTracker.CalculationService.Models;

/// <summary>
/// Response DTO for interstellar object information
/// </summary>
public record InterstellarObjectResponse
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Official designation (e.g., 2I/2019 Q4)
    /// </summary>
    public required string Designation { get; init; }

    /// <summary>
    /// Common name (e.g., Borisov)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Discovery date
    /// </summary>
    public required DateTimeOffset DiscoveryDate { get; init; }

    /// <summary>
    /// Name of discoverer
    /// </summary>
    public required string Discoverer { get; init; }

    /// <summary>
    /// Estimated diameter in meters
    /// </summary>
    public required double EstimatedDiameterMeters { get; init; }

    /// <summary>
    /// Calculated position at the requested date
    /// </summary>
    public required PositionDto Position { get; init; }

    /// <summary>
    /// Calculated velocity at the requested date
    /// </summary>
    public required VelocityDto Velocity { get; init; }

    /// <summary>
    /// Date and time for which the position was calculated
    /// </summary>
    public required DateTime CalculationDate { get; init; }

    /// <summary>
    /// Orbital eccentricity (> 1 for interstellar objects)
    /// </summary>
    public required double Eccentricity { get; init; }
}
