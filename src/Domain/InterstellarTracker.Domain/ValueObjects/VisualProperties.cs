namespace InterstellarTracker.Domain.ValueObjects;

/// <summary>
/// Visual properties for rendering a celestial body.
/// </summary>
/// <remarks>
/// Immutable value object for visual representation.
/// Kept in domain layer as it's intrinsic to the concept of a celestial body.
/// </remarks>
public sealed class VisualProperties : IEquatable<VisualProperties>
{
    /// <summary>
    /// Primary color (RGB, 0-1 range).
    /// </summary>
    public RgbColor Color { get; }

    /// <summary>
    /// Albedo (reflectivity, 0-1 range). 0 = black body, 1 = perfect reflector.
    /// </summary>
    public double Albedo { get; }

    /// <summary>
    /// Whether the body emits light (like a star).
    /// </summary>
    public bool IsLuminous { get; }

    /// <summary>
    /// Scale factor for visualization (to make small bodies visible).
    /// </summary>
    public double RenderScaleFactor { get; }

    /// <summary>
    /// Whether to render orbital path.
    /// </summary>
    public bool ShowOrbit { get; }

    /// <summary>
    /// Color for orbital path visualization.
    /// </summary>
    public RgbColor OrbitColor { get; }

    public VisualProperties(
        RgbColor color,
        double albedo,
        bool isLuminous,
        double renderScaleFactor = 1.0,
        bool showOrbit = true,
        RgbColor? orbitColor = null)
    {
        if (albedo < 0 || albedo > 1)
            throw new ArgumentOutOfRangeException(nameof(albedo), "Albedo must be between 0 and 1.");

        if (renderScaleFactor <= 0)
            throw new ArgumentOutOfRangeException(nameof(renderScaleFactor), "Scale factor must be positive.");

        Color = color ?? throw new ArgumentNullException(nameof(color));
        Albedo = albedo;
        IsLuminous = isLuminous;
        RenderScaleFactor = renderScaleFactor;
        ShowOrbit = showOrbit;
        OrbitColor = orbitColor ?? color;
    }

    /// <summary>
    /// Create visual properties for a star.
    /// </summary>
    public static VisualProperties Star(RgbColor color, double scaleFactor = 1.0) =>
        new(color, 0.0, true, scaleFactor, false);

    /// <summary>
    /// Create visual properties for a planet.
    /// </summary>
    public static VisualProperties Planet(RgbColor color, double albedo, double scaleFactor = 1.0) =>
        new(color, albedo, false, scaleFactor, true);

    /// <summary>
    /// Create visual properties for a comet/interstellar object.
    /// </summary>
    public static VisualProperties Comet(RgbColor color, double scaleFactor = 10.0) =>
        new(color, 0.04, false, scaleFactor, true, new RgbColor(0.8, 0.8, 1.0)); // Blue-ish orbit

    public bool Equals(VisualProperties? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Color.Equals(other.Color) &&
               Math.Abs(Albedo - other.Albedo) < 1e-6 &&
               IsLuminous == other.IsLuminous &&
               Math.Abs(RenderScaleFactor - other.RenderScaleFactor) < 1e-6;
    }

    public override bool Equals(object? obj) => Equals(obj as VisualProperties);

    public override int GetHashCode() =>
        HashCode.Combine(Color, Albedo, IsLuminous, RenderScaleFactor);
}

/// <summary>
/// RGB color value object.
/// </summary>
public sealed class RgbColor : IEquatable<RgbColor>
{
    public double R { get; }
    public double G { get; }
    public double B { get; }

    public RgbColor(double r, double g, double b)
    {
        if (r < 0 || r > 1 || g < 0 || g > 1 || b < 0 || b > 1)
            throw new ArgumentOutOfRangeException("RGB values must be between 0 and 1.");

        R = r;
        G = g;
        B = b;
    }

    // Common colors
    public static RgbColor White => new(1.0, 1.0, 1.0);
    public static RgbColor Yellow => new(1.0, 1.0, 0.0);
    public static RgbColor Blue => new(0.0, 0.5, 1.0);
    public static RgbColor Red => new(1.0, 0.3, 0.2);
    public static RgbColor Orange => new(1.0, 0.6, 0.2);
    public static RgbColor Gray => new(0.5, 0.5, 0.5);
    public static RgbColor IceBlue => new(0.8, 0.9, 1.0);

    public bool Equals(RgbColor? other)
    {
        if (other is null) return false;
        return Math.Abs(R - other.R) < 1e-6 &&
               Math.Abs(G - other.G) < 1e-6 &&
               Math.Abs(B - other.B) < 1e-6;
    }

    public override bool Equals(object? obj) => Equals(obj as RgbColor);

    public override int GetHashCode() => HashCode.Combine(R, G, B);

    public System.Numerics.Vector3 ToVector3() => new((float)R, (float)G, (float)B);
}
