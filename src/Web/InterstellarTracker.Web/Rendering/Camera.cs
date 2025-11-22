using Silk.NET.Maths;

namespace InterstellarTracker.Web.Rendering;

/// <summary>
/// Orbital camera for 3D solar system visualization.
/// Supports orbit, zoom, and pan controls.
/// </summary>
public class Camera
{
    private float _distance = 50.0f; // AU from target
    private float _azimuth = 45.0f; // degrees (horizontal rotation)
    private float _elevation = 30.0f; // degrees (vertical rotation)
    private Vector3D<float> _target = Vector3D<float>.Zero; // looking at origin

    /// <summary>
    /// Field of view in degrees.
    /// </summary>
    public float Fov { get; set; } = 45.0f;

    /// <summary>
    /// Aspect ratio (width / height).
    /// </summary>
    public float AspectRatio { get; set; } = 16.0f / 9.0f;

    /// <summary>
    /// Near clipping plane.
    /// </summary>
    public float Near { get; set; } = 0.1f;

    /// <summary>
    /// Far clipping plane (very large for solar system).
    /// </summary>
    public float Far { get; set; } = 10000.0f;

    /// <summary>
    /// Current camera position in world space.
    /// </summary>
    public Vector3D<float> Position
    {
        get
        {
            // Convert spherical coordinates to Cartesian
            float azimuthRad = _azimuth * MathF.PI / 180.0f;
            float elevationRad = _elevation * MathF.PI / 180.0f;

            float x = _distance * MathF.Cos(elevationRad) * MathF.Cos(azimuthRad);
            float y = _distance * MathF.Sin(elevationRad);
            float z = _distance * MathF.Cos(elevationRad) * MathF.Sin(azimuthRad);

            return _target + new Vector3D<float>(x, y, z);
        }
    }

    /// <summary>
    /// Gets the view matrix (world to camera space).
    /// </summary>
    public Matrix4X4<float> GetViewMatrix()
    {
        Vector3D<float> position = Position;
        Vector3D<float> up = Vector3D<float>.UnitY;

        return Matrix4X4.CreateLookAt(position, _target, up);
    }

    /// <summary>
    /// Gets the projection matrix (camera to clip space).
    /// </summary>
    public Matrix4X4<float> GetProjectionMatrix()
    {
        return Matrix4X4.CreatePerspectiveFieldOfView(
            Fov * MathF.PI / 180.0f,
            AspectRatio,
            Near,
            Far
        );
    }

    /// <summary>
    /// Orbits the camera around the target.
    /// </summary>
    /// <param name="deltaAzimuth">Horizontal rotation in degrees.</param>
    /// <param name="deltaElevation">Vertical rotation in degrees.</param>
    public void Orbit(float deltaAzimuth, float deltaElevation)
    {
        _azimuth += deltaAzimuth;
        _elevation += deltaElevation;

        // Clamp elevation to prevent flipping
        _elevation = Math.Clamp(_elevation, -89.0f, 89.0f);

        // Wrap azimuth
        _azimuth = _azimuth % 360.0f;
    }

    /// <summary>
    /// Zooms the camera in/out.
    /// </summary>
    /// <param name="delta">Zoom amount (negative = zoom in).</param>
    public void Zoom(float delta)
    {
        _distance += delta;
        _distance = Math.Max(1.0f, _distance); // Min distance
        _distance = Math.Min(5000.0f, _distance); // Max distance
    }

    /// <summary>
    /// Pans the camera (moves the target).
    /// </summary>
    /// <param name="deltaX">Horizontal pan.</param>
    /// <param name="deltaY">Vertical pan.</param>
    public void Pan(float deltaX, float deltaY)
    {
        // Get camera right and up vectors
        Vector3D<float> forward = Vector3D.Normalize(_target - Position);
        Vector3D<float> right = Vector3D.Normalize(Vector3D.Cross(forward, Vector3D<float>.UnitY));
        Vector3D<float> up = Vector3D.Normalize(Vector3D.Cross(right, forward));

        // Move target
        _target += right * deltaX + up * deltaY;
    }

    /// <summary>
    /// Resets camera to default position.
    /// </summary>
    public void Reset()
    {
        _distance = 50.0f;
        _azimuth = 45.0f;
        _elevation = 30.0f;
        _target = Vector3D<float>.Zero;
    }

    /// <summary>
    /// Focuses camera on a specific target.
    /// </summary>
    public void FocusOn(Vector3D<float> target, float distance = 10.0f)
    {
        _target = target;
        _distance = distance;
    }
}
