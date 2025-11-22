using InterstellarTracker.Domain.Entities;
using InterstellarTracker.Infrastructure.Persistence;

namespace InterstellarTracker.Integration.Tests.Infrastructure;

/// <summary>
/// Integration tests for InMemoryCelestialBodyRepository.
/// Validates that the repository correctly populates and retrieves
/// all celestial bodies including planets, dwarf planets, comets,
/// and interstellar objects.
/// </summary>
public class InMemoryCelestialBodyRepositoryTests
{
    private readonly InMemoryCelestialBodyRepository _repository;

    public InMemoryCelestialBodyRepositoryTests()
    {
        _repository = new InMemoryCelestialBodyRepository();
    }

    /// <summary>
    /// Verifies that all expected celestial bodies are loaded:
    /// - 1 star (Sun)
    /// - 8 planets (Mercury through Neptune)
    /// - 5 dwarf planets (Pluto, Ceres, Eris, Makemake, Haumea)
    /// - 5 comets (Halley, Hale-Bopp, Encke, Churyumov-Gerasimenko, Hyakutake)
    /// Total: 19 bodies (interstellar objects are accessed separately)
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturn_AllCelestialBodies()
    {
        // Act
        var bodies = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(bodies);
        Assert.Equal(19, bodies.Count());

        // Verify counts by type
        Assert.Single(bodies, b => b.Type == CelestialBodyType.Star);
        Assert.Equal(8, bodies.Count(b => b.Type == CelestialBodyType.Planet));
        Assert.Equal(5, bodies.Count(b => b.Type == CelestialBodyType.DwarfPlanet));
        Assert.Equal(5, bodies.Count(b => b.Type == CelestialBodyType.Comet));
    }

    /// <summary>
    /// Validates that the Sun exists with correct properties.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Sun_ShouldHaveCorrectProperties()
    {
        // Act
        var sun = await _repository.GetByIdAsync("sun");

        // Assert
        Assert.NotNull(sun);
        Assert.Equal("Sun", sun.Name);
        Assert.Equal(CelestialBodyType.Star, sun.Type);
        Assert.Equal(1.98892e30, sun.MassKg, 0.00001e30);
        Assert.Null(sun.OrbitalElements); // Sun doesn't orbit
    }

    /// <summary>
    /// Validates Earth's orbital elements are reasonable.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Earth_ShouldHaveCorrectOrbit()
    {
        // Act
        var earth = await _repository.GetByIdAsync("earth");

        // Assert
        Assert.NotNull(earth);
        Assert.Equal("Earth", earth.Name);
        Assert.Equal(CelestialBodyType.Planet, earth.Type);

        // Verify orbital elements
        var orbit = earth.OrbitalElements;
        Assert.NotNull(orbit);

        // Semi-major axis should be ~1 AU (149.6 million km)
        Assert.InRange(orbit.SemiMajorAxisMeters, 1.49e11, 1.50e11);

        // Eccentricity should be very small (~0.0167)
        Assert.InRange(orbit.Eccentricity, 0.016, 0.018);

        // Inclination should be small (orbital plane)
        Assert.InRange(orbit.InclinationRadians, 0, 0.01);
    }

    /// <summary>
    /// Validates that Halley's Comet has hyperbolic-like high eccentricity.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_HalleyComet_ShouldHaveHighEccentricity()
    {
        // Act
        var halley = await _repository.GetByIdAsync("halley");

        // Assert
        Assert.NotNull(halley);
        Assert.Equal("1P/Halley", halley.Name);
        Assert.Equal(CelestialBodyType.Comet, halley.Type);

        // Halley's Comet has e ≈ 0.967 (very elliptical)
        var orbit = halley.OrbitalElements;
        Assert.NotNull(orbit);
        Assert.InRange(orbit.Eccentricity, 0.95, 0.98);
    }

    /// <summary>
    /// Validates that all three interstellar objects exist with hyperbolic orbits.
    /// </summary>
    [Fact]
    public async Task GetInterstellarObjectByIdAsync_ShouldReturnAll3Objects()
    {
        // Arrange
        var oumuamuaId = "oumuamua";
        var borisovaId = "borisov";
        var atlasId = "atlas";

        // Act
        var oumuamua = await _repository.GetInterstellarObjectByIdAsync(oumuamuaId);
        var borisov = await _repository.GetInterstellarObjectByIdAsync(borisovaId);
        var atlas = await _repository.GetInterstellarObjectByIdAsync(atlasId);

        // Assert
        Assert.NotNull(oumuamua);
        Assert.NotNull(borisov);
        Assert.NotNull(atlas);

        // Verify hyperbolic orbits (e > 1)
        Assert.True(oumuamua.OrbitalElements.Eccentricity > 1.0);
        Assert.True(borisov.OrbitalElements.Eccentricity > 1.0);
        Assert.True(atlas.OrbitalElements.Eccentricity > 1.0);

        // Verify designations
        Assert.Equal("1I/2017 U1", oumuamua.Designation);
        Assert.Equal("2I/2019 Q4", borisov.Designation);
        Assert.Equal("3I/2024 S1", atlas.Designation);
    }

    /// <summary>
    /// Validates that 3I/ATLAS has the highest eccentricity ever recorded (6.14).
    /// </summary>
    [Fact]
    public async Task GetInterstellarObjectByIdAsync_Atlas_ShouldHaveRecordEccentricity()
    {
        // Act
        var atlas = await _repository.GetInterstellarObjectByIdAsync("atlas");

        // Assert
        Assert.NotNull(atlas);
        Assert.Equal("3I/2024 S1", atlas.Designation);
        Assert.Equal("3I/ATLAS", atlas.Name);

        // e = 6.14 is the highest ever recorded
        Assert.InRange(atlas.OrbitalElements.Eccentricity, 6.0, 6.2);

        // Discovered July 1, 2025
        Assert.Equal(new DateTime(2025, 7, 1), atlas.DiscoveryDate.DateTime);

        // Discoverer
        Assert.Equal("ATLAS Survey", atlas.Discoverer);
    }

    /// <summary>
    /// Validates dwarf planets (Pluto, Ceres, Eris, Makemake, Haumea).
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldIncludeAllDwarfPlanets()
    {
        // Act
        var bodies = await _repository.GetAllAsync();
        var dwarfPlanets = bodies.Where(b => b.Type == CelestialBodyType.DwarfPlanet).ToList();

        // Assert
        Assert.Equal(5, dwarfPlanets.Count);

        // Verify names
        var names = dwarfPlanets.Select(p => p.Name).ToList();
        Assert.Contains("Pluto", names);
        Assert.Contains("Ceres", names);
        Assert.Contains("Eris", names);
        Assert.Contains("Makemake", names);
        Assert.Contains("Haumea", names);
    }

    /// <summary>
    /// Validates that querying a non-existent ID returns null.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync("99999999-9999-9999-9999-999999999999");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Validates that querying a planet as an interstellar object returns null.
    /// </summary>
    [Fact]
    public async Task GetInterstellarObjectByIdAsync_PlanetId_ShouldReturnNull()
    {
        // Arrange - Earth's ID
        var earthId = "earth";

        // Act
        var result = await _repository.GetInterstellarObjectByIdAsync(earthId);

        // Assert
        Assert.Null(result);
    }
}
