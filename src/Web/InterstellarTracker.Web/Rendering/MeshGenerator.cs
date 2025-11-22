using Silk.NET.OpenGL;

namespace InterstellarTracker.Web.Rendering;

/// <summary>
/// Generates 3D mesh data for geometric primitives.
/// Vertex data includes positions and normals for lighting.
/// </summary>
public static class MeshGenerator
{
    /// <summary>
    /// Generates a UV sphere mesh.
    /// </summary>
    /// <param name="radius">Sphere radius.</param>
    /// <param name="latitudeBands">Number of horizontal bands.</param>
    /// <param name="longitudeBands">Number of vertical bands.</param>
    /// <returns>Tuple of (vertices, indices) where vertices are interleaved position+normal.</returns>
    public static (float[] vertices, uint[] indices) GenerateSphere(float radius, int latitudeBands, int longitudeBands)
    {
        List<float> vertices = new();
        List<uint> indices = new();

        // Generate vertices
        for (int lat = 0; lat <= latitudeBands; lat++)
        {
            float theta = lat * MathF.PI / latitudeBands;
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);

            for (int lon = 0; lon <= longitudeBands; lon++)
            {
                float phi = lon * 2 * MathF.PI / longitudeBands;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                // Position
                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;

                // Normal (same as position for unit sphere)
                float nx = x;
                float ny = y;
                float nz = z;

                // Scale by radius
                vertices.Add(x * radius);
                vertices.Add(y * radius);
                vertices.Add(z * radius);

                // Normal
                vertices.Add(nx);
                vertices.Add(ny);
                vertices.Add(nz);
            }
        }

        // Generate indices
        for (int lat = 0; lat < latitudeBands; lat++)
        {
            for (int lon = 0; lon < longitudeBands; lon++)
            {
                uint first = (uint)(lat * (longitudeBands + 1) + lon);
                uint second = (uint)(first + longitudeBands + 1);

                // Triangle 1
                indices.Add(first);
                indices.Add(second);
                indices.Add(first + 1);

                // Triangle 2
                indices.Add(second);
                indices.Add(second + 1);
                indices.Add(first + 1);
            }
        }

        return (vertices.ToArray(), indices.ToArray());
    }

    /// <summary>
    /// Creates OpenGL buffers (VAO, VBO, EBO) for a mesh.
    /// </summary>
    /// <param name="gl">OpenGL context.</param>
    /// <param name="vertices">Vertex data (interleaved position+normal).</param>
    /// <param name="indices">Index data.</param>
    /// <returns>Tuple of (VAO, VBO, EBO, indexCount).</returns>
    public static unsafe (uint vao, uint vbo, uint ebo, int indexCount) CreateBuffers(
        GL gl,
        float[] vertices,
        uint[] indices)
    {
        // Generate buffers
        uint vao = gl.GenVertexArray();
        uint vbo = gl.GenBuffer();
        uint ebo = gl.GenBuffer();

        gl.BindVertexArray(vao);

        // Upload vertex data
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (void* v = &vertices[0])
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }

        // Upload index data
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        fixed (void* i = &indices[0])
        {
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
        }

        // Configure vertex attributes
        int stride = 6 * sizeof(float); // position (3) + normal (3)

        // Position attribute (location = 0)
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
        gl.EnableVertexAttribArray(0);

        // Normal attribute (location = 1)
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);

        // Unbind
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindVertexArray(0);

        return (vao, vbo, ebo, indices.Length);
    }

    /// <summary>
    /// Generates line vertices for an elliptical orbit.
    /// </summary>
    /// <param name="a">Semi-major axis.</param>
    /// <param name="e">Eccentricity.</param>
    /// <param name="segments">Number of line segments.</param>
    /// <returns>Array of 3D positions for line strip.</returns>
    public static float[] GenerateOrbitPath(float a, float e, int segments = 360)
    {
        List<float> vertices = new();

        for (int i = 0; i <= segments; i++)
        {
            // True anomaly from 0 to 2π
            float nu = i * 2 * MathF.PI / segments;

            // Orbit equation: r = a(1 - e²) / (1 + e*cos(nu))
            float r = a * (1 - e * e) / (1 + e * MathF.Cos(nu));

            // Convert to Cartesian (orbit in XZ plane for now)
            float x = r * MathF.Cos(nu);
            float y = 0;
            float z = r * MathF.Sin(nu);

            vertices.Add(x);
            vertices.Add(y);
            vertices.Add(z);
        }

        return vertices.ToArray();
    }

    /// <summary>
    /// Creates OpenGL line buffer for orbit visualization.
    /// </summary>
    public static unsafe (uint vao, uint vbo, int vertexCount) CreateLineBuffers(GL gl, float[] vertices)
    {
        uint vao = gl.GenVertexArray();
        uint vbo = gl.GenBuffer();

        gl.BindVertexArray(vao);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (void* v = &vertices[0])
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }

        // Position attribute only
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
        gl.EnableVertexAttribArray(0);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindVertexArray(0);

        return (vao, vbo, vertices.Length / 3);
    }
}
