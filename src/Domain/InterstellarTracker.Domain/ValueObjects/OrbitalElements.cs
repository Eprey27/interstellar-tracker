namespace InterstellarTracker.Domain.ValueObjects;

/// <summary>
/// Keplerian orbital elements that fully define an orbit.
/// </summary>
/// <remarks>
/// Based on the six classical orbital elements used in celestial mechanics.
/// Immutable value object for domain model purity.
/// All angles in radians, distances in meters.
/// </remarks>
public sealed class OrbitalElements : IEquatable<OrbitalElements>
{
    /// <summary>
    /// Semi-major axis in meters (average distance from parent body).
    /// </summary>
    public double SemiMajorAxisMeters { get; }

    /// <summary>
    /// Eccentricity (0 = circular, 0-1 = elliptical, 1 = parabolic, >1 = hyperbolic).
    /// </summary>
    public double Eccentricity { get; }

    /// <summary>
    /// Inclination in radians (tilt of orbit relative to reference plane).
    /// </summary>
    public double InclinationRadians { get; }

    /// <summary>
    /// Longitude of ascending node in radians.
    /// </summary>
    public double LongitudeOfAscendingNodeRadians { get; }

    /// <summary>
    /// Argument of periapsis in radians (where orbit is closest to parent).
    /// </summary>
    public double ArgumentOfPeriapsisRadians { get; }

    /// <summary>
    /// Mean anomaly at epoch in radians (position in orbit at reference time).
    /// </summary>
    public double MeanAnomalyAtEpochRadians { get; }

    /// <summary>
    /// Epoch (reference time) as Julian Date.
    /// </summary>
    public double EpochJulianDate { get; }

    /// <summary>
    /// Gravitational parameter (GM) of the parent body in m³/s².
    /// </summary>
    public double GravitationalParameter { get; }

    public OrbitalElements(
        double semiMajorAxisMeters,
        double eccentricity,
        double inclinationRadians,
        double longitudeOfAscendingNodeRadians,
        double argumentOfPeriapsisRadians,
        double meanAnomalyAtEpochRadians,
        double epochJulianDate,
        double gravitationalParameter)
    {
        // Semi-major axis can be negative for hyperbolic orbits (e > 1)
        if (semiMajorAxisMeters == 0)
            throw new ArgumentOutOfRangeException(nameof(semiMajorAxisMeters), "Cannot be zero.");

        if (eccentricity < 0)
            throw new ArgumentOutOfRangeException(nameof(eccentricity));

        if (gravitationalParameter <= 0)
            throw new ArgumentOutOfRangeException(nameof(gravitationalParameter));

        SemiMajorAxisMeters = semiMajorAxisMeters;
        Eccentricity = eccentricity;
        InclinationRadians = inclinationRadians;
        LongitudeOfAscendingNodeRadians = longitudeOfAscendingNodeRadians;
        ArgumentOfPeriapsisRadians = argumentOfPeriapsisRadians;
        MeanAnomalyAtEpochRadians = meanAnomalyAtEpochRadians;
        EpochJulianDate = epochJulianDate;
        GravitationalParameter = gravitationalParameter;
    }

    /// <summary>
    /// Calculate the position vector at a given time using Keplerian orbital mechanics.
    /// </summary>
    /// <param name="julianDate">Time for calculation.</param>
    /// <returns>Position vector in meters from parent body.</returns>
    public Vector3 CalculatePosition(double julianDate)
    {
        // Time since epoch in seconds
        var timeSinceEpoch = (julianDate - EpochJulianDate) * 86400.0;

        // Mean motion (rad/s)
        var meanMotion = Math.Sqrt(GravitationalParameter / Math.Pow(SemiMajorAxisMeters, 3));

        // Current mean anomaly
        var meanAnomaly = MeanAnomalyAtEpochRadians + meanMotion * timeSinceEpoch;

        // Solve Kepler's equation for eccentric anomaly (Newton-Raphson iteration)
        var eccentricAnomaly = SolveKeplersEquation(meanAnomaly, Eccentricity);

        // True anomaly
        var trueAnomaly = 2.0 * Math.Atan2(
            Math.Sqrt(1.0 + Eccentricity) * Math.Sin(eccentricAnomaly / 2.0),
            Math.Sqrt(1.0 - Eccentricity) * Math.Cos(eccentricAnomaly / 2.0)
        );

        // Distance from parent
        var distance = SemiMajorAxisMeters * (1.0 - Eccentricity * Math.Cos(eccentricAnomaly));

        // Position in orbital plane
        var x = distance * Math.Cos(trueAnomaly);
        var y = distance * Math.Sin(trueAnomaly);

        // Rotate to ecliptic coordinates
        return RotateToEcliptic(x, y, 0);
    }

    /// <summary>
    /// Calculate the velocity vector at a given time.
    /// </summary>
    public Vector3 CalculateVelocity(double julianDate)
    {
        // Time since epoch
        var timeSinceEpoch = (julianDate - EpochJulianDate) * 86400.0;
        var meanMotion = Math.Sqrt(GravitationalParameter / Math.Pow(SemiMajorAxisMeters, 3));
        var meanAnomaly = MeanAnomalyAtEpochRadians + meanMotion * timeSinceEpoch;
        var eccentricAnomaly = SolveKeplersEquation(meanAnomaly, Eccentricity);

        // Velocity in orbital plane
        var distance = SemiMajorAxisMeters * (1.0 - Eccentricity * Math.Cos(eccentricAnomaly));
        var velocityMagnitude = Math.Sqrt(GravitationalParameter * SemiMajorAxisMeters) / distance;

        var vx = -velocityMagnitude * Math.Sin(eccentricAnomaly);
        var vy = velocityMagnitude * Math.Sqrt(1.0 - Eccentricity * Eccentricity) * Math.Cos(eccentricAnomaly);

        return RotateToEcliptic(vx, vy, 0);
    }

    /// <summary>
    /// Solve Kepler's equation: M = E - e*sin(E) using Newton-Raphson method.
    /// </summary>
    private static double SolveKeplersEquation(double meanAnomaly, double eccentricity, int maxIterations = 10)
    {
        var E = meanAnomaly; // Initial guess

        for (int i = 0; i < maxIterations; i++)
        {
            var deltaE = (E - eccentricity * Math.Sin(E) - meanAnomaly) / (1.0 - eccentricity * Math.Cos(E));
            E -= deltaE;

            if (Math.Abs(deltaE) < 1e-10)
                break;
        }

        return E;
    }

    /// <summary>
    /// Rotate from orbital plane to ecliptic coordinates.
    /// </summary>
    private Vector3 RotateToEcliptic(double x, double y, double z)
    {
        // Rotation matrices for orbital elements
        var cosW = Math.Cos(ArgumentOfPeriapsisRadians);
        var sinW = Math.Sin(ArgumentOfPeriapsisRadians);
        var cosO = Math.Cos(LongitudeOfAscendingNodeRadians);
        var sinO = Math.Sin(LongitudeOfAscendingNodeRadians);
        var cosI = Math.Cos(InclinationRadians);
        var sinI = Math.Sin(InclinationRadians);

        // Combined rotation matrix elements
        var xComp = x * (cosW * cosO - sinW * sinO * cosI) - y * (sinW * cosO + cosW * sinO * cosI);
        var yComp = x * (cosW * sinO + sinW * cosO * cosI) + y * (cosW * cosO * cosI - sinW * sinO);
        var zComp = x * (sinW * sinI) + y * (cosW * sinI);

        return new Vector3(xComp, yComp, zComp);
    }

    public bool Equals(OrbitalElements? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Math.Abs(SemiMajorAxisMeters - other.SemiMajorAxisMeters) < 1e-6 &&
               Math.Abs(Eccentricity - other.Eccentricity) < 1e-10 &&
               Math.Abs(InclinationRadians - other.InclinationRadians) < 1e-10 &&
               Math.Abs(EpochJulianDate - other.EpochJulianDate) < 1e-6;
    }

    public override bool Equals(object? obj) => Equals(obj as OrbitalElements);

    public override int GetHashCode() =>
        HashCode.Combine(SemiMajorAxisMeters, Eccentricity, InclinationRadians, EpochJulianDate);
}
