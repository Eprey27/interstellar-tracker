using Xunit;

namespace InterstellarTracker.Integration.Tests.Rendering;

/// <summary>
/// Tests for mesh generation utilities.
/// </summary>
public class MeshGeneratorTests
{
    [Fact]
    public void GenerateOrbitPath_ZeroInclination_ShouldBeInXYPlane()
    {
        // Arrange
        float a = 1.0f; // 1 AU
        float e = 0.0f; // Circular
        float i = 0.0f; // Zero inclination - should be in XY plane
        float omega = 0.0f;
        float w = 0.0f;
        int segments = 360;

        // Act
        var vertices = InterstellarTracker.Web.Rendering.MeshGenerator.GenerateOrbitPath(a, e, i, omega, w, segments);

        // Assert - all Z coordinates should be approximately zero
        for (int j = 0; j < vertices.Length; j += 3)
        {
            float x = vertices[j];
            float y = vertices[j + 1];
            float z = vertices[j + 2];

            // Z should be essentially zero for ecliptic plane
            Assert.True(Math.Abs(z) < 0.0001f, $"Point {j / 3}: Z={z} should be near zero for ecliptic plane orbit");

            // Radius should be ~1 AU for circular orbit
            float radius = MathF.Sqrt(x * x + y * y);
            Assert.True(Math.Abs(radius - 1.0f) < 0.01f, $"Point {j / 3}: radius={radius} should be ~1.0 AU");
        }
    }

    [Fact]
    public void GenerateOrbitPath_90DegreeInclination_ShouldBeVertical()
    {
        // Arrange
        float a = 1.0f;
        float e = 0.0f;
        float i = MathF.PI / 2.0f; // 90 degrees - orbit perpendicular to ecliptic
        float omega = 0.0f;
        float w = 0.0f;
        int segments = 360;

        // Act
        var vertices = InterstellarTracker.Web.Rendering.MeshGenerator.GenerateOrbitPath(a, e, i, omega, w, segments);

        // Assert - orbit should be in XZ plane (Y varies, Z varies, but constrained)
        for (int j = 0; j < vertices.Length; j += 3)
        {
            float x = vertices[j];
            float y = vertices[j + 1];
            float z = vertices[j + 2];

            // For 90° inclination with Ω=0, orbit should be in XZ plane
            // So Y should be near zero
            Assert.True(Math.Abs(y) < 0.0001f, $"Point {j / 3}: Y={y} should be near zero for 90° inclined orbit with Ω=0");

            float radius = MathF.Sqrt(x * x + z * z);
            Assert.True(Math.Abs(radius - 1.0f) < 0.01f, $"Point {j / 3}: radius={radius} should be ~1.0 AU");
        }
    }

    [Fact]
    public void GenerateOrbitPath_EarthLikeOrbit_ShouldBeNearlyFlat()
    {
        // Arrange - Earth-like orbital elements
        float a = 1.0f; // 1 AU
        float e = 0.0167f; // Earth's eccentricity
        float i = 0.0f; // ~0° inclination relative to ecliptic (by definition)
        float omega = 0.0f;
        float w = 1.796f; // ~103° argument of periapsis (radians)
        int segments = 360;

        // Act
        var vertices = InterstellarTracker.Web.Rendering.MeshGenerator.GenerateOrbitPath(a, e, i, omega, w, segments);

        // Assert
        float maxZ = 0;
        float minZ = 0;
        for (int j = 2; j < vertices.Length; j += 3)
        {
            float z = vertices[j];
            maxZ = Math.Max(maxZ, z);
            minZ = Math.Min(minZ, z);
        }

        // With zero inclination, Z should remain essentially zero
        Assert.True(Math.Abs(maxZ) < 0.0001f, $"Max Z={maxZ} should be near zero");
        Assert.True(Math.Abs(minZ) < 0.0001f, $"Min Z={minZ} should be near zero");
    }

    [Fact]
    public void GenerateOrbitPath_EllipticalOrbit_CorrectSemiMajorAxis()
    {
        // Arrange
        float a = 2.5f; // 2.5 AU
        float e = 0.5f; // Moderate eccentricity
        float i = 0.0f;
        float omega = 0.0f;
        float w = 0.0f;
        int segments = 360;

        // Act
        var vertices = InterstellarTracker.Web.Rendering.MeshGenerator.GenerateOrbitPath(a, e, i, omega, w, segments);

        // Assert - find perihelion and aphelion distances
        float minR = float.MaxValue;
        float maxR = float.MinValue;

        for (int j = 0; j < vertices.Length; j += 3)
        {
            float x = vertices[j];
            float y = vertices[j + 1];
            float z = vertices[j + 2];
            float r = MathF.Sqrt(x * x + y * y + z * z);

            minR = Math.Min(minR, r);
            maxR = Math.Max(maxR, r);
        }

        // Perihelion = a(1-e), Aphelion = a(1+e)
        float expectedPerihelion = a * (1 - e); // 2.5 * 0.5 = 1.25
        float expectedAphelion = a * (1 + e);   // 2.5 * 1.5 = 3.75

        Assert.True(Math.Abs(minR - expectedPerihelion) < 0.01f,
            $"Perihelion distance {minR} should be ~{expectedPerihelion}");
        Assert.True(Math.Abs(maxR - expectedAphelion) < 0.01f,
            $"Aphelion distance {maxR} should be ~{expectedAphelion}");
    }
}
