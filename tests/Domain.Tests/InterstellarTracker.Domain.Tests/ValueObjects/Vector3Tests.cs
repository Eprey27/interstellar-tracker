using InterstellarTracker.Domain.ValueObjects;

namespace InterstellarTracker.Domain.Tests.ValueObjects;

/// <summary>
/// Unit tests for Vector3 value object.
/// </summary>
public class Vector3Tests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var vector = new Vector3(1.0, 2.0, 3.0);

        Assert.Equal(1.0, vector.X);
        Assert.Equal(2.0, vector.Y);
        Assert.Equal(3.0, vector.Z);
    }

    [Fact]
    public void Magnitude_CalculatesCorrectly()
    {
        var vector = new Vector3(3.0, 4.0, 0.0);

        Assert.Equal(5.0, vector.Magnitude, precision: 6);
    }

    [Fact]
    public void Normalized_ReturnsUnitVector()
    {
        var vector = new Vector3(3.0, 4.0, 0.0);
        var normalized = vector.Normalized;

        Assert.Equal(1.0, normalized.Magnitude, precision: 6);
        Assert.Equal(0.6, normalized.X, precision: 6);
        Assert.Equal(0.8, normalized.Y, precision: 6);
    }

    [Fact]
    public void Addition_WorksCorrectly()
    {
        var v1 = new Vector3(1.0, 2.0, 3.0);
        var v2 = new Vector3(4.0, 5.0, 6.0);

        var result = v1 + v2;

        Assert.Equal(5.0, result.X);
        Assert.Equal(7.0, result.Y);
        Assert.Equal(9.0, result.Z);
    }

    [Fact]
    public void Subtraction_WorksCorrectly()
    {
        var v1 = new Vector3(5.0, 7.0, 9.0);
        var v2 = new Vector3(1.0, 2.0, 3.0);

        var result = v1 - v2;

        Assert.Equal(4.0, result.X);
        Assert.Equal(5.0, result.Y);
        Assert.Equal(6.0, result.Z);
    }

    [Fact]
    public void ScalarMultiplication_WorksCorrectly()
    {
        var vector = new Vector3(1.0, 2.0, 3.0);

        var result = vector * 2.0;

        Assert.Equal(2.0, result.X);
        Assert.Equal(4.0, result.Y);
        Assert.Equal(6.0, result.Z);
    }

    [Fact]
    public void DotProduct_CalculatesCorrectly()
    {
        var v1 = new Vector3(1.0, 2.0, 3.0);
        var v2 = new Vector3(4.0, 5.0, 6.0);

        var dot = v1.Dot(v2);

        Assert.Equal(32.0, dot); // 1*4 + 2*5 + 3*6 = 32
    }

    [Fact]
    public void CrossProduct_CalculatesCorrectly()
    {
        var v1 = new Vector3(1.0, 0.0, 0.0);
        var v2 = new Vector3(0.0, 1.0, 0.0);

        var cross = v1.Cross(v2);

        Assert.Equal(0.0, cross.X);
        Assert.Equal(0.0, cross.Y);
        Assert.Equal(1.0, cross.Z);
    }

    [Fact]
    public void DistanceTo_CalculatesCorrectly()
    {
        var v1 = new Vector3(0.0, 0.0, 0.0);
        var v2 = new Vector3(3.0, 4.0, 0.0);

        var distance = v1.DistanceTo(v2);

        Assert.Equal(5.0, distance, precision: 6);
    }

    [Fact]
    public void StaticVectors_HaveCorrectValues()
    {
        Assert.Equal(new Vector3(0, 0, 0), Vector3.Zero);
        Assert.Equal(new Vector3(1, 0, 0), Vector3.UnitX);
        Assert.Equal(new Vector3(0, 1, 0), Vector3.UnitY);
        Assert.Equal(new Vector3(0, 0, 1), Vector3.UnitZ);
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var v1 = new Vector3(1.0, 2.0, 3.0);
        var v2 = new Vector3(1.0, 2.0, 3.0);
        var v3 = new Vector3(1.0, 2.0, 3.1);

        Assert.True(v1 == v2);
        Assert.False(v1 == v3);
        Assert.True(v1.Equals(v2));
        Assert.False(v1.Equals(v3));
    }
}
