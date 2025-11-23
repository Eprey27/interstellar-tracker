namespace InterstellarTracker.VisualizationService.Models;

/// <summary>
/// Represents a 3D vector in space (position or velocity)
/// </summary>
/// <param name="X">X coordinate (AU or AU/day)</param>
/// <param name="Y">Y coordinate (AU or AU/day)</param>
/// <param name="Z">Z coordinate (AU or AU/day)</param>
public record Vector3D(double X, double Y, double Z);

/// <summary>
/// Response containing full trajectory data for an interstellar object
/// </summary>
/// <param name="ObjectId">Object identifier (e.g., "2I/Borisov")</param>
/// <param name="Points">List of trajectory points</param>
/// <param name="GeneratedAt">Timestamp when trajectory was generated</param>
public record TrajectoryResponse(
    string ObjectId,
    List<TrajectoryPoint> Points,
    DateTime GeneratedAt
);

/// <summary>
/// Single point in a trajectory
/// </summary>
/// <param name="Timestamp">Date/time of this position</param>
/// <param name="Position">3D position in AU (J2000 heliocentric)</param>
/// <param name="Velocity">3D velocity in AU/day</param>
public record TrajectoryPoint(
    DateTime Timestamp,
    Vector3D Position,
    Vector3D Velocity
);

/// <summary>
/// Response containing position at a specific time
/// </summary>
/// <param name="ObjectId">Object identifier</param>
/// <param name="Timestamp">Timestamp of the position</param>
/// <param name="Position">3D position in AU</param>
/// <param name="Velocity">3D velocity in AU/day</param>
/// <param name="DistanceFromSun">Distance from Sun in AU</param>
public record PositionResponse(
    string ObjectId,
    DateTime Timestamp,
    Vector3D Position,
    Vector3D Velocity,
    double DistanceFromSun
);
