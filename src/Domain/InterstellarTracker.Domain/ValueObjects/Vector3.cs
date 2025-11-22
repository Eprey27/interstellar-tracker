namespace InterstellarTracker.Domain.ValueObjects;

/// <summary>
/// Represents a 3D vector for positions, velocities, and directions.
/// </summary>
/// <remarks>
/// Immutable value object. Thread-safe.
/// Using custom type instead of System.Numerics.Vector3 for domain purity.
/// </remarks>
public readonly struct Vector3 : IEquatable<Vector3>
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 Zero => new(0, 0, 0);
    public static Vector3 UnitX => new(1, 0, 0);
    public static Vector3 UnitY => new(0, 1, 0);
    public static Vector3 UnitZ => new(0, 0, 1);

    /// <summary>
    /// Calculate the magnitude (length) of the vector.
    /// </summary>
    public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>
    /// Calculate the squared magnitude (avoids sqrt for performance).
    /// </summary>
    public double MagnitudeSquared => X * X + Y * Y + Z * Z;

    /// <summary>
    /// Return a normalized (unit length) version of this vector.
    /// </summary>
    public Vector3 Normalized
    {
        get
        {
            var mag = Magnitude;
            return mag > 0 ? new Vector3(X / mag, Y / mag, Z / mag) : Zero;
        }
    }

    /// <summary>
    /// Calculate the dot product with another vector.
    /// </summary>
    public double Dot(Vector3 other) => X * other.X + Y * other.Y + Z * other.Z;

    /// <summary>
    /// Calculate the cross product with another vector.
    /// </summary>
    public Vector3 Cross(Vector3 other) => new(
        Y * other.Z - Z * other.Y,
        Z * other.X - X * other.Z,
        X * other.Y - Y * other.X
    );

    /// <summary>
    /// Calculate the distance to another vector.
    /// </summary>
    public double DistanceTo(Vector3 other) => (this - other).Magnitude;

    public static Vector3 operator +(Vector3 a, Vector3 b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3 operator -(Vector3 a, Vector3 b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3 operator *(Vector3 v, double scalar) =>
        new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3 operator *(double scalar, Vector3 v) =>
        v * scalar;

    public static Vector3 operator /(Vector3 v, double scalar) =>
        new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public static Vector3 operator -(Vector3 v) =>
        new(-v.X, -v.Y, -v.Z);

    public bool Equals(Vector3 other) =>
        Math.Abs(X - other.X) < 1e-10 &&
        Math.Abs(Y - other.Y) < 1e-10 &&
        Math.Abs(Z - other.Z) < 1e-10;

    public override bool Equals(object? obj) =>
        obj is Vector3 other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(X, Y, Z);

    public static bool operator ==(Vector3 left, Vector3 right) =>
        left.Equals(right);

    public static bool operator !=(Vector3 left, Vector3 right) =>
        !left.Equals(right);

    public override string ToString() =>
        $"({X:F2}, {Y:F2}, {Z:F2})";

    /// <summary>
    /// Convert to System.Numerics.Vector3 for graphics operations.
    /// </summary>
    public System.Numerics.Vector3 ToNumerics() =>
        new((float)X, (float)Y, (float)Z);
}
