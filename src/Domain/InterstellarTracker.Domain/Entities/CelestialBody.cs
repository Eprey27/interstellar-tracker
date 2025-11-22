using InterstellarTracker.Domain.ValueObjects;

namespace InterstellarTracker.Domain.Entities;

/// <summary>
/// Represents a celestial body in the solar system (planet, star, moon, asteroid, comet).
/// </summary>
/// <remarks>
/// This is a core domain entity following Clean Architecture principles.
/// Immutable by design with value-based equality.
/// </remarks>
public sealed class CelestialBody : IEquatable<CelestialBody>
{
    /// <summary>
    /// Unique identifier for the celestial body (e.g., "sun", "earth", "2i-borisov").
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Human-readable name of the celestial body.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Type of celestial body (Star, Planet, Moon, Asteroid, Comet, InterstellarObject).
    /// </summary>
    public CelestialBodyType Type { get; }

    /// <summary>
    /// Mass in kilograms.
    /// </summary>
    public double MassKg { get; }

    /// <summary>
    /// Radius in meters.
    /// </summary>
    public double RadiusMeters { get; }

    /// <summary>
    /// Orbital parameters (null for central body like the Sun).
    /// </summary>
    public OrbitalElements? OrbitalElements { get; }

    /// <summary>
    /// Visual properties for rendering.
    /// </summary>
    public VisualProperties Visual { get; }

    public CelestialBody(
        string id,
        string name,
        CelestialBodyType type,
        double massKg,
        double radiusMeters,
        OrbitalElements? orbitalElements,
        VisualProperties visual)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty.", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        if (massKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(massKg), "Mass must be positive.");

        if (radiusMeters <= 0)
            throw new ArgumentOutOfRangeException(nameof(radiusMeters), "Radius must be positive.");

        Id = id;
        Name = name;
        Type = type;
        MassKg = massKg;
        RadiusMeters = radiusMeters;
        OrbitalElements = orbitalElements;
        Visual = visual ?? throw new ArgumentNullException(nameof(visual));
    }

    /// <summary>
    /// Calculate the position of this body at a given time using orbital mechanics.
    /// </summary>
    /// <param name="julianDate">Julian date for position calculation.</param>
    /// <returns>Position vector in meters from parent body (heliocentric for planets).</returns>
    public Vector3 CalculatePosition(double julianDate)
    {
        if (OrbitalElements == null)
            return Vector3.Zero; // Central body (Sun) at origin

        return OrbitalElements.CalculatePosition(julianDate);
    }

    /// <summary>
    /// Calculate the velocity of this body at a given time.
    /// </summary>
    /// <param name="julianDate">Julian date for velocity calculation.</param>
    /// <returns>Velocity vector in meters per second.</returns>
    public Vector3 CalculateVelocity(double julianDate)
    {
        if (OrbitalElements == null)
            return Vector3.Zero;

        return OrbitalElements.CalculateVelocity(julianDate);
    }

    public bool Equals(CelestialBody? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as CelestialBody);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(CelestialBody? left, CelestialBody? right) =>
        Equals(left, right);

    public static bool operator !=(CelestialBody? left, CelestialBody? right) =>
        !Equals(left, right);
}

/// <summary>
/// Types of celestial bodies in the simulation.
/// </summary>
public enum CelestialBodyType
{
    Star,
    Planet,
    DwarfPlanet,
    Moon,
    Asteroid,
    Comet,
    InterstellarObject
}
