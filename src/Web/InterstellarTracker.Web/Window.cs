using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using InterstellarTracker.Application.Common.Interfaces;
using InterstellarTracker.Domain.Entities;
using InterstellarTracker.Web.Rendering;
using Shader = InterstellarTracker.Web.Rendering.Shader;

namespace InterstellarTracker.Web;

/// <summary>
/// Main window class for the Interstellar Tracker visualization.
/// Manages OpenGL context, input, and rendering loop.
/// </summary>
public class Window
{
    private readonly IWindow _window;
    private readonly ICelestialBodyRepository _repository;
    private GL _gl = null!;
    private IInputContext _input = null!;
    private Camera _camera = null!;
    private Shader _shader = null!;
    private Shader _lineShader = null!;

    // Sphere mesh (shared by all celestial bodies)
    private uint _sphereVAO;
    private uint _sphereVBO;
    private uint _sphereEBO;
    private int _sphereIndexCount;

    // Celestial body data
    private List<CelestialBody> _bodies = new();
    private List<InterstellarObject> _interstellarObjects = new();

    // Orbit paths
    private Dictionary<string, (uint vao, uint vbo, int count)> _orbitBuffers = new();

    // Input state
    private Vector2D<float> _lastMousePos;
    private bool _leftMouseDown;
    private bool _rightMouseDown;
    private bool _middleMouseDown;

    // Time simulation
    private double _julianDate = 2451545.0; // J2000.0 epoch
    private float _timeSpeed = 1.0f; // 1 day per second
    private bool _paused = false;

    public Window(ICelestialBodyRepository repository)
    {
        _repository = repository;

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1920, 1080);
        options.Title = "Interstellar Tracker - 3D Solar System Visualization";
        options.VSync = true;

        _window = Silk.NET.Windowing.Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Update += OnUpdate;
        _window.Closing += OnClose;
        _window.Resize += OnResize;
    }

    public void Run()
    {
        _window.Run();
    }

    private async void OnLoad()
    {
        Console.WriteLine("Initializing Interstellar Tracker...");

        // Get OpenGL context
        _gl = _window.CreateOpenGL();
        _input = _window.CreateInput();

        // Setup input
        foreach (var mouse in _input.Mice)
        {
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnMouseScroll;
        }

        foreach (var keyboard in _input.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
        }

        // Configure OpenGL
        _gl.ClearColor(0.01f, 0.01f, 0.02f, 1.0f); // Dark space background
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(TriangleFace.Back);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Create shaders
        _shader = new Shader(_gl, ShaderSources.VertexShader, ShaderSources.FragmentShader);
        _lineShader = new Shader(_gl, ShaderSources.LineVertexShader, ShaderSources.LineFragmentShader);

        // Create camera
        _camera = new Camera
        {
            AspectRatio = (float)_window.Size.X / _window.Size.Y
        };
        _camera.Reset();

        // Generate sphere mesh
        var (vertices, indices) = MeshGenerator.GenerateSphere(1.0f, 32, 32);
        (_sphereVAO, _sphereVBO, _sphereEBO, _sphereIndexCount) = MeshGenerator.CreateBuffers(_gl, vertices, indices);

        // Load celestial bodies
        await LoadCelestialBodies();

        Console.WriteLine($"Loaded {_bodies.Count} celestial bodies and {_interstellarObjects.Count} interstellar objects");
        Console.WriteLine("Controls:");
        Console.WriteLine("  Left Mouse + Drag: Orbit camera");
        Console.WriteLine("  Mouse Wheel: Zoom");
        Console.WriteLine("  Middle Mouse + Drag: Pan");
        Console.WriteLine("  Space: Pause/Resume");
        Console.WriteLine("  R: Reset camera");
        Console.WriteLine("  +/-: Increase/Decrease time speed");
        Console.WriteLine("Ready!");
    }

    private async Task LoadCelestialBodies()
    {
        _bodies = (await _repository.GetAllAsync()).ToList();
        _interstellarObjects = (await _repository.GetAllInterstellarObjectsAsync()).ToList();

        // Generate orbit paths for bodies with orbital elements
        foreach (var body in _bodies.Where(b => b.OrbitalElements != null))
        {
            var orbit = body.OrbitalElements!;

            // Convert to AU for visualization (scale down from meters)
            float aAU = (float)(orbit.SemiMajorAxisMeters / 149_597_870_700.0);
            float e = (float)orbit.Eccentricity;

            if (e < 1.0f) // Only elliptical orbits for now
            {
                var vertices = MeshGenerator.GenerateOrbitPath(aAU, e, 360);
                var (vao, vbo, count) = MeshGenerator.CreateLineBuffers(_gl, vertices);
                _orbitBuffers[body.Id] = (vao, vbo, count);
            }
        }

        // For interstellar objects, generate hyperbolic trajectories
        foreach (var obj in _interstellarObjects)
        {
            var orbit = obj.OrbitalElements;
            float aAU = (float)(orbit.SemiMajorAxisMeters / 149_597_870_700.0);
            float e = (float)orbit.Eccentricity;

            if (e > 1.0f) // Hyperbolic
            {
                // For now, just draw a portion of the hyperbola near perihelion
                var vertices = MeshGenerator.GenerateOrbitPath(Math.Abs(aAU), e, 180);
                var (vao, vbo, count) = MeshGenerator.CreateLineBuffers(_gl, vertices);
                _orbitBuffers[obj.Id] = (vao, vbo, count);
            }
        }
    }

    private void OnUpdate(double deltaTime)
    {
        if (!_paused)
        {
            // Advance simulation time
            _julianDate += deltaTime * _timeSpeed;
        }
    }

    private void OnRender(double deltaTime)
    {
        // Clear
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Get camera matrices
        var view = _camera.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix();

        // Render orbit paths first (transparent lines)
        _lineShader.Use();
        _lineShader.SetMatrix4("uView", view);
        _lineShader.SetMatrix4("uProjection", projection);

        foreach (var (bodyId, (vao, _, count)) in _orbitBuffers)
        {
            _gl.BindVertexArray(vao);
            _lineShader.SetVector3("uLineColor", new Vector3D<float>(0.3f, 0.3f, 0.4f));
            _gl.DrawArrays(PrimitiveType.LineStrip, 0, (uint)count);
        }

        // Render celestial bodies
        _shader.Use();
        _shader.SetMatrix4("uView", view);
        _shader.SetMatrix4("uProjection", projection);
        _shader.SetVector3("uViewPos", _camera.Position);
        _shader.SetVector3("uLightPos", Vector3D<float>.Zero); // Sun at origin

        // Render Sun
        var sun = _bodies.FirstOrDefault(b => b.Id == "sun");
        if (sun != null)
        {
            RenderCelestialBody(sun, Vector3D<float>.Zero, isEmissive: true);
        }

        // Render other bodies
        foreach (var body in _bodies.Where(b => b.Id != "sun" && b.OrbitalElements != null))
        {
            var position = CalculatePosition(body.OrbitalElements!, _julianDate);
            RenderCelestialBody(body, position, isEmissive: false);
        }

        // Render interstellar objects
        foreach (var obj in _interstellarObjects)
        {
            var position = CalculatePosition(obj.OrbitalElements, _julianDate);
            RenderInterstellarObject(obj, position);
        }
    }

    private void RenderCelestialBody(CelestialBody body, Vector3D<float> position, bool isEmissive)
    {
        // Scale: logarithmic to make small bodies visible
        float radiusAU = (float)(body.RadiusMeters / 149_597_870_700.0);
        float visualScale = MathF.Max(0.05f, MathF.Log10(radiusAU + 1) * 0.2f);

        var model = Matrix4X4.CreateScale(visualScale) * Matrix4X4.CreateTranslation(position);

        _shader.SetMatrix4("uModel", model);
        _shader.SetVector3("uObjectColor", GetBodyColor(body));
        _shader.SetFloat("uEmissive", isEmissive ? 1.0f : 0.0f);

        _gl.BindVertexArray(_sphereVAO);
        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, (uint)_sphereIndexCount, DrawElementsType.UnsignedInt, null);
        }
    }

    private Vector3D<float> CalculatePosition(Domain.ValueObjects.OrbitalElements orbit, double julianDate)
    {
        try
        {
            // Use domain's orbital calculation
            var position3D = orbit.CalculatePosition(julianDate);

            // Convert from meters to AU
            float x = (float)(position3D.X / 149_597_870_700.0);
            float y = (float)(position3D.Y / 149_597_870_700.0);
            float z = (float)(position3D.Z / 149_597_870_700.0);

            return new Vector3D<float>(x, y, z);
        }
        catch
        {
            // If calculation fails (e.g., hyperbolic orbit issues), return origin
            return Vector3D<float>.Zero;
        }
    }

    private void RenderInterstellarObject(InterstellarObject obj, Vector3D<float> position)
    {
        // Visualize interstellar objects as small bright spheres
        float visualScale = 0.1f; // Small but visible

        var model = Matrix4X4.CreateScale(visualScale) * Matrix4X4.CreateTranslation(position);

        _shader.SetMatrix4("uModel", model);
        _shader.SetVector3("uObjectColor", GetInterstellarObjectColor(obj));
        _shader.SetFloat("uEmissive", 0.5f); // Slightly emissive for visibility

        _gl.BindVertexArray(_sphereVAO);
        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, (uint)_sphereIndexCount, DrawElementsType.UnsignedInt, null);
        }
    }

    private Vector3D<float> GetBodyColor(CelestialBody body)
    {
        var visual = body.Visual;
        return new Vector3D<float>(
            (float)visual.Color.R,
            (float)visual.Color.G,
            (float)visual.Color.B
        );
    }

    private Vector3D<float> GetInterstellarObjectColor(InterstellarObject obj)
    {
        var visual = obj.Visual;
        return new Vector3D<float>(
            (float)visual.Color.R,
            (float)visual.Color.G,
            (float)visual.Color.B
        );
    }

    // Input handlers
    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        if (button == MouseButton.Left) _leftMouseDown = true;
        if (button == MouseButton.Right) _rightMouseDown = true;
        if (button == MouseButton.Middle) _middleMouseDown = true;
        _lastMousePos = new Vector2D<float>(mouse.Position.X, mouse.Position.Y);
    }

    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        if (button == MouseButton.Left) _leftMouseDown = false;
        if (button == MouseButton.Right) _rightMouseDown = false;
        if (button == MouseButton.Middle) _middleMouseDown = false;
    }

    private void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
    {
        var delta = new Vector2D<float>(position.X, position.Y) - _lastMousePos;

        if (_leftMouseDown)
        {
            // Orbit
            _camera.Orbit(delta.X * 0.2f, -delta.Y * 0.2f);
        }
        else if (_middleMouseDown)
        {
            // Pan
            _camera.Pan(-delta.X * 0.05f, delta.Y * 0.05f);
        }

        _lastMousePos = new Vector2D<float>(position.X, position.Y);
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        _camera.Zoom(-scroll.Y * 2.0f);
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int code)
    {
        switch (key)
        {
            case Key.Space:
                _paused = !_paused;
                Console.WriteLine(_paused ? "Paused" : "Resumed");
                break;
            case Key.R:
                _camera.Reset();
                _julianDate = 2451545.0;
                _timeSpeed = 1.0f;
                Console.WriteLine("Reset camera and time");
                break;
            case Key.Equal: // + key
            case Key.KeypadAdd:
                _timeSpeed *= 2.0f;
                Console.WriteLine($"Time speed: {_timeSpeed}x");
                break;
            case Key.Minus:
            case Key.KeypadSubtract:
                _timeSpeed /= 2.0f;
                Console.WriteLine($"Time speed: {_timeSpeed}x");
                break;
            case Key.Escape:
                _window.Close();
                break;
        }
    }

    private void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _camera.AspectRatio = (float)size.X / size.Y;
    }

    private void OnClose()
    {
        // Cleanup
        _gl.DeleteVertexArray(_sphereVAO);
        _gl.DeleteBuffer(_sphereVBO);
        _gl.DeleteBuffer(_sphereEBO);

        foreach (var (vao, vbo, _) in _orbitBuffers.Values)
        {
            _gl.DeleteVertexArray(vao);
            _gl.DeleteBuffer(vbo);
        }

        _shader.Dispose();
        _lineShader.Dispose();
        _input.Dispose();
        _gl.Dispose();
    }
}
