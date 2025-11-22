using InterstellarTracker.Domain.Entities;
using InterstellarTracker.Domain.ValueObjects;

namespace InterstellarTracker.Domain.Tests.Entities;

/// <summary>
/// Unit tests for InterstellarObject entity.
/// </summary>
public class InterstellarObjectTests
{
    private const double SunGM = 1.32712440018e20;

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        var orbitalElements = CreateTestOrbitalElements();
        var visual = VisualProperties.Comet(RgbColor.IceBlue);

        var obj = new InterstellarObject(
            id: "test-object",
            designation: "TEST/2024",
            name: "Test Object",
            orbitalElements: orbitalElements,
            discoveryDate: DateTimeOffset.UtcNow,
            discoverer: "Test Astronomer",
            visual: visual,
            estimatedDiameterMeters: 500.0
        );

        Assert.Equal("test-object", obj.Id);
        Assert.Equal("TEST/2024", obj.Designation);
        Assert.Equal("Test Object", obj.Name);
        Assert.Equal(500.0, obj.EstimatedDiameterMeters);
    }

    [Fact]
    public void Constructor_NullOrEmptyId_ThrowsException()
    {
        var orbitalElements = CreateTestOrbitalElements();
        var visual = VisualProperties.Comet(RgbColor.IceBlue);

        Assert.Throws<ArgumentException>(() =>
            new InterstellarObject("", "TEST/2024", "Test", orbitalElements,
                DateTimeOffset.UtcNow, "Test", visual, 500.0)
        );
    }

    [Fact]
    public void Constructor_NullOrbitalElements_ThrowsException()
    {
        var visual = VisualProperties.Comet(RgbColor.IceBlue);

        Assert.Throws<ArgumentNullException>(() =>
            new InterstellarObject("test", "TEST/2024", "Test", null!,
                DateTimeOffset.UtcNow, "Test", visual, 500.0)
        );
    }

    [Fact]
    public void Constructor_NegativeDiameter_ThrowsException()
    {
        var orbitalElements = CreateTestOrbitalElements();
        var visual = VisualProperties.Comet(RgbColor.IceBlue);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InterstellarObject("test", "TEST/2024", "Test", orbitalElements,
                DateTimeOffset.UtcNow, "Test", visual, -100.0)
        );
    }

    [Fact]
    public void CalculatePosition_ReturnsValidVector()
    {
        var borisov = InterstellarObject.CreateBorisov();
        var julianDate = 2458826.5; // Epoch date

        var position = borisov.CalculatePosition(julianDate);

        Assert.NotEqual(double.NaN, position.X);
        Assert.NotEqual(double.NaN, position.Y);
        Assert.NotEqual(double.NaN, position.Z);
        Assert.True(position.Magnitude > 0, "Position magnitude should be positive");
    }

    [Fact]
    public void CalculateVelocity_ReturnsValidVector()
    {
        var borisov = InterstellarObject.CreateBorisov();
        var julianDate = 2458826.5;

        var velocity = borisov.CalculateVelocity(julianDate);

        Assert.NotEqual(double.NaN, velocity.X);
        Assert.NotEqual(double.NaN, velocity.Y);
        Assert.NotEqual(double.NaN, velocity.Z);
        Assert.True(velocity.Magnitude > 0, "Velocity magnitude should be positive");
    }

    [Fact]
    public void CreateBorisov_ReturnsValidObject()
    {
        var borisov = InterstellarObject.CreateBorisov();

        Assert.Equal("2i-borisov", borisov.Id);
        Assert.Equal("2I/Borisov", borisov.Designation);
        Assert.Equal("Borisov", borisov.Name);
        Assert.Equal("Gennady Borisov", borisov.Discoverer);
        Assert.True(borisov.OrbitalElements.Eccentricity > 1.0); // Hyperbolic
        Assert.Equal(800.0, borisov.EstimatedDiameterMeters);
    }

    [Fact]
    public void CreateBorisov_HasHyperbolicOrbit()
    {
        var borisov = InterstellarObject.CreateBorisov();

        // Interstellar objects have eccentricity > 1 (hyperbolic orbits)
        Assert.True(borisov.OrbitalElements.Eccentricity > 1.0);
    }

    private static OrbitalElements CreateTestOrbitalElements()
    {
        return new OrbitalElements(
            semiMajorAxisMeters: -1.496e11,
            eccentricity: 2.5,
            inclinationRadians: 0.5,
            longitudeOfAscendingNodeRadians: 1.0,
            argumentOfPeriapsisRadians: 2.0,
            meanAnomalyAtEpochRadians: 0.0,
            epochJulianDate: 2451545.0,
            gravitationalParameter: SunGM
        );
    }
}
