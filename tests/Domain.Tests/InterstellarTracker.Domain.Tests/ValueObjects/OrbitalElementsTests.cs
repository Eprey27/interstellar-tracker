using InterstellarTracker.Domain.ValueObjects;

namespace InterstellarTracker.Domain.Tests.ValueObjects;

/// <summary>
/// Unit tests for OrbitalElements value object and Keplerian mechanics.
/// </summary>
public class OrbitalElementsTests
{
    private const double AU = 1.496e11; // Astronomical Unit in meters
    private const double SunGM = 1.32712440018e20; // Sun's gravitational parameter (m³/s²)

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        var elements = new OrbitalElements(
            semiMajorAxisMeters: AU,
            eccentricity: 0.0167,
            inclinationRadians: 0.0,
            longitudeOfAscendingNodeRadians: 0.0,
            argumentOfPeriapsisRadians: 0.0,
            meanAnomalyAtEpochRadians: 0.0,
            epochJulianDate: 2451545.0,
            gravitationalParameter: SunGM
        );

        Assert.NotNull(elements);
        Assert.Equal(AU, elements.SemiMajorAxisMeters);
        Assert.Equal(0.0167, elements.Eccentricity);
    }

    [Fact]
    public void Constructor_ZeroSemiMajorAxis_ThrowsException()
    {
        // Zero semi-major axis is invalid (but negative is allowed for hyperbolic orbits)
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OrbitalElements(0, 0.5, 0, 0, 0, 0, 2451545.0, SunGM)
        );
    }

    [Fact]
    public void Constructor_NegativeEccentricity_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OrbitalElements(AU, -0.5, 0, 0, 0, 0, 2451545.0, SunGM)
        );
    }

    [Fact]
    public void CalculatePosition_CircularOrbit_ReturnsCorrectRadius()
    {
        // Circular orbit (e=0) at epoch should be at semi-major axis distance
        var elements = new OrbitalElements(
            semiMajorAxisMeters: AU,
            eccentricity: 0.0,
            inclinationRadians: 0.0,
            longitudeOfAscendingNodeRadians: 0.0,
            argumentOfPeriapsisRadians: 0.0,
            meanAnomalyAtEpochRadians: 0.0,
            epochJulianDate: 2451545.0,
            gravitationalParameter: SunGM
        );

        var position = elements.CalculatePosition(2451545.0);
        var distance = position.Magnitude;

        // For circular orbit, distance should equal semi-major axis
        Assert.Equal(AU, distance, precision: 5);
    }

    [Fact]
    public void CalculatePosition_EllipticalOrbit_VariesWithTime()
    {
        // Earth-like elliptical orbit
        var elements = new OrbitalElements(
            semiMajorAxisMeters: AU,
            eccentricity: 0.0167,
            inclinationRadians: 0.0,
            longitudeOfAscendingNodeRadians: 0.0,
            argumentOfPeriapsisRadians: 0.0,
            meanAnomalyAtEpochRadians: 0.0,
            epochJulianDate: 2451545.0,
            gravitationalParameter: SunGM
        );

        var positionAtEpoch = elements.CalculatePosition(2451545.0);
        var positionLater = elements.CalculatePosition(2451545.0 + 91.25); // ~1/4 orbit

        // Positions should differ
        Assert.NotEqual(positionAtEpoch, positionLater);
        
        // Both should be roughly at orbital distance
        Assert.InRange(positionAtEpoch.Magnitude, AU * 0.9, AU * 1.1);
        Assert.InRange(positionLater.Magnitude, AU * 0.9, AU * 1.1);
    }

    [Fact]
    public void CalculateVelocity_ReturnsNonZeroVector()
    {
        var elements = new OrbitalElements(
            semiMajorAxisMeters: AU,
            eccentricity: 0.0167,
            inclinationRadians: 0.0,
            longitudeOfAscendingNodeRadians: 0.0,
            argumentOfPeriapsisRadians: 0.0,
            meanAnomalyAtEpochRadians: 0.0,
            epochJulianDate: 2451545.0,
            gravitationalParameter: SunGM
        );

        var velocity = elements.CalculateVelocity(2451545.0);

        // Earth's orbital velocity is ~30 km/s
        Assert.InRange(velocity.Magnitude, 25000, 35000);
    }

    [Fact]
    public void HyperbolicOrbit_EccentricityGreaterThanOne_IsValid()
    {
        // Interstellar object with hyperbolic trajectory
        var elements = new OrbitalElements(
            semiMajorAxisMeters: -AU, // Negative for hyperbolic
            eccentricity: 3.0,
            inclinationRadians: 0.5,
            longitudeOfAscendingNodeRadians: 1.0,
            argumentOfPeriapsisRadians: 2.0,
            meanAnomalyAtEpochRadians: 0.0,
            epochJulianDate: 2451545.0,
            gravitationalParameter: SunGM
        );

        Assert.NotNull(elements);
        Assert.True(elements.Eccentricity > 1.0);
        
        // Should still calculate position without error
        var position = elements.CalculatePosition(2451545.0);
        Assert.NotNull(position);
    }

    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        var e1 = new OrbitalElements(AU, 0.5, 0.1, 0.2, 0.3, 0.4, 2451545.0, SunGM);
        var e2 = new OrbitalElements(AU, 0.5, 0.1, 0.2, 0.3, 0.4, 2451545.0, SunGM);

        Assert.True(e1.Equals(e2));
    }

    [Fact]
    public void Equality_DifferentValues_ReturnsFalse()
    {
        var e1 = new OrbitalElements(AU, 0.5, 0.1, 0.2, 0.3, 0.4, 2451545.0, SunGM);
        var e2 = new OrbitalElements(AU * 1.5, 0.5, 0.1, 0.2, 0.3, 0.4, 2451545.0, SunGM);

        Assert.False(e1.Equals(e2));
    }
}
