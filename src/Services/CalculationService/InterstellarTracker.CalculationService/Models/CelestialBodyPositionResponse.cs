namespace InterstellarTracker.CalculationService.Models;

/// <summary>
/// Response DTO for celestial body position calculations
/// </summary>
public record CelestialBodyPositionResponse
{
    /// <summary>
    /// Unique identifier of the celestial body
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Name of the celestial body
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type of celestial body (Star, Planet, Comet, etc.)
    /// </summary>
    public required string Type { get; init; }

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
}

/// <summary>
/// 3D position coordinates (in meters from the Sun)
/// </summary>
public record PositionDto
{
    /// <summary>
    /// X coordinate (meters)
    /// </summary>
    public required double X { get; init; }

    /// <summary>
    /// Y coordinate (meters)
    /// </summary>
    public required double Y { get; init; }

    /// <summary>
    /// Z coordinate (meters)
    /// </summary>
    public required double Z { get; init; }

    /// <summary>
    /// Distance from origin (magnitude, meters)
    /// </summary>
    public required double Magnitude { get; init; }
}

/// <summary>
/// 3D velocity vector (in meters per second)
/// </summary>
public record VelocityDto
{
    /// <summary>
    /// X component (m/s)
    /// </summary>
    public required double X { get; init; }

    /// <summary>
    /// Y component (m/s)
    /// </summary>
    public required double Y { get; init; }

    /// <summary>
    /// Z component (m/s)
    /// </summary>
    public required double Z { get; init; }

    /// <summary>
    /// Speed (magnitude, m/s)
    /// </summary>
    public required double Magnitude { get; init; }
}
