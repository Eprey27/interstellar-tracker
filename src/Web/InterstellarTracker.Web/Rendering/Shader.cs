using System.Diagnostics.CodeAnalysis;
using Silk.NET.OpenGL;
using InterstellarTracker.Domain.Exceptions;

namespace InterstellarTracker.Web.Rendering;

/// <summary>
/// Manages OpenGL shader programs (vertex + fragment shaders).
/// Handles compilation, linking, and uniform variable management.
/// </summary>
public class Shader : IDisposable
{
    private readonly GL _gl;
    private readonly uint _program;
    private bool _disposed;

    public Shader(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;

        // Compile vertex shader
        uint vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);

        // Compile fragment shader
        uint fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

        // Link program
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);
        _gl.LinkProgram(_program);

        // Check linking errors
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            string infoLog = _gl.GetProgramInfoLog(_program);
            var ex = new RenderingException($"Shader program linking failed: {infoLog}")
            {
                Operation = "shader linking",
                ErrorDetails = infoLog
            };
            throw ex;
        }

        // Clean up shaders (already linked into program)
        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        // Check compilation errors
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            var ex = new ShaderCompilationException($"{type} compilation failed: {infoLog}")
            {
                ShaderType = type.ToString(),
                CompilationLog = infoLog
            };
            throw ex;
        }

        return shader;
    }

    /// <summary>
    /// Activates this shader program for rendering.
    /// </summary>
    public void Use()
    {
        _gl.UseProgram(_program);
    }

    /// <summary>
    /// Sets a mat4 uniform variable.
    /// </summary>
    /// <remarks>
    /// Uses unsafe code for direct memory access required by OpenGL API.
    /// This is necessary for efficient matrix data transfer to GPU.
    /// </remarks>
    [SuppressMessage("Security", "S6640:Using unsafe code is security-sensitive", Justification = "Required for OpenGL matrix pointer operations")]
    public unsafe void SetMatrix4(string name, Silk.NET.Maths.Matrix4X4<float> matrix)
    {
        int location = _gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            Console.WriteLine($"Warning: Uniform '{name}' not found in shader");
            return;
        }
        _gl.UniformMatrix4(location, 1, false, (float*)&matrix);
    }

    /// <summary>
    /// Sets a vec3 uniform variable.
    /// </summary>
    public void SetVector3(string name, Silk.NET.Maths.Vector3D<float> vector)
    {
        int location = _gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            Console.WriteLine($"Warning: Uniform '{name}' not found in shader");
            return;
        }
        _gl.Uniform3(location, vector.X, vector.Y, vector.Z);
    }

    /// <summary>
    /// Sets a float uniform variable.
    /// </summary>
    public void SetFloat(string name, float value)
    {
        int location = _gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            Console.WriteLine($"Warning: Uniform '{name}' not found in shader");
            return;
        }
        _gl.Uniform1(location, value);
    }

    /// <summary>
    /// Releases all resources used by the Shader.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the Shader and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources if any
            }

            _gl.DeleteProgram(_program);
            _disposed = true;
        }
    }
}
