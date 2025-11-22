using InterstellarTracker.Domain.ValueObjects;

namespace InterstellarTracker.Domain.Entities;

/// <summary>
/// Represents an interstellar object (e.g., 2I/Borisov) with discovery metadata.
/// </summary>
/// <remarks>
/// Interstellar objects have hyperbolic orbits (eccentricity > 1) and pass through
/// our solar system only once. This class emphasizes discovery metadata and
/// trajectory tracking specific to such objects.
/// </remarks>
public sealed class InterstellarObject
{
    /// <summary>
    /// Unique identifier (e.g., "2i-borisov").
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Official designation (e.g., "2I/Borisov", "1I/'Oumuamua").
    /// </summary>
    public string Designation { get; }

    /// <summary>
    /// Common name for the object.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Orbital elements defining the trajectory through the solar system.
    /// </summary>
    public OrbitalElements OrbitalElements { get; }

    /// <summary>
    /// Date when the object was discovered.
    /// </summary>
    public DateTimeOffset DiscoveryDate { get; }

    /// <summary>
    /// Name of the person or observatory that discovered the object.
    /// </summary>
    public string Discoverer { get; }

    /// <summary>
    /// Visual rendering properties.
    /// </summary>
    public VisualProperties Visual { get; }

    /// <summary>
    /// Estimated diameter in meters (often uncertain for interstellar objects).
    /// </summary>
    public double EstimatedDiameterMeters { get; }

    public InterstellarObject(
        string id,
        string designation,
        string name,
        OrbitalElements orbitalElements,
        DateTimeOffset discoveryDate,
        string discoverer,
        VisualProperties visual,
        double estimatedDiameterMeters)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty.", nameof(id));
        
        if (string.IsNullOrWhiteSpace(designation))
            throw new ArgumentException("Designation cannot be null or empty.", nameof(designation));

        if (estimatedDiameterMeters <= 0)
            throw new ArgumentOutOfRangeException(nameof(estimatedDiameterMeters), "Diameter must be positive.");

        Id = id;
        Designation = designation;
        Name = string.IsNullOrWhiteSpace(name) ? designation : name;
        OrbitalElements = orbitalElements ?? throw new ArgumentNullException(nameof(orbitalElements));
        DiscoveryDate = discoveryDate;
        Discoverer = discoverer ?? string.Empty;
        Visual = visual ?? throw new ArgumentNullException(nameof(visual));
        EstimatedDiameterMeters = estimatedDiameterMeters;
    }

    /// <summary>
    /// Calculate position at a given time.
    /// </summary>
    /// <param name="julianDate">Julian date for calculation.</param>
    /// <returns>Position vector in meters from the Sun.</returns>
    public Vector3 CalculatePosition(double julianDate) =>
        OrbitalElements.CalculatePosition(julianDate);

    /// <summary>
    /// Calculate velocity at a given time.
    /// </summary>
    /// <param name="julianDate">Julian date for calculation.</param>
    /// <returns>Velocity vector in meters per second.</returns>
    public Vector3 CalculateVelocity(double julianDate) =>
        OrbitalElements.CalculateVelocity(julianDate);

    /// <summary>
    /// Factory method for creating 2I/Borisov with actual orbital data.
    /// </summary>
    /// <remarks>
    /// Orbital elements from JPL Small-Body Database (approximate values).
    /// Epoch: 2019-Dec-08 (JD 2458826.5)
    /// </remarks>
    public static InterstellarObject CreateBorisov()
    {
        // Simplified orbital elements for 2I/Borisov
        // Real implementation would use precise JPL data
        var orbitalElements = new OrbitalElements(
            semiMajorAxisMeters: -0.8516 * 1.496e11,  // Negative for hyperbolic orbit, ~0.85 AU
            eccentricity: 3.3569,                      // Hyperbolic (>1)
            inclinationRadians: 44.053 * Math.PI / 180.0,
            longitudeOfAscendingNodeRadians: 308.15 * Math.PI / 180.0,
            argumentOfPeriapsisRadians: 209.13 * Math.PI / 180.0,
            meanAnomalyAtEpochRadians: 0.0,
            epochJulianDate: 2458826.5,                // 2019-Dec-08
            gravitationalParameter: 1.32712440018e20   // Sun's GM (m³/s²)
        );

        var visual = VisualProperties.Comet(
            RgbColor.IceBlue,
            scaleFactor: 20.0  // Make visible despite small size
        );

        return new InterstellarObject(
            id: "2i-borisov",
            designation: "2I/Borisov",
            name: "Borisov",
            orbitalElements: orbitalElements,
            discoveryDate: new DateTimeOffset(2019, 8, 30, 0, 0, 0, TimeSpan.Zero),
            discoverer: "Gennady Borisov",
            visual: visual,
            estimatedDiameterMeters: 800.0  // ~800m diameter estimate
        );
    }
}
