using System;

namespace InterstellarTracker.Domain.Exceptions;

/// <summary>
/// Thrown when a shader fails to compile.
/// This represents a recoverable error in shader compilation process.
/// </summary>
public class ShaderCompilationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderCompilationException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ShaderCompilationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderCompilationException"/> class with a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ShaderCompilationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the shader type that failed to compile (e.g., "Vertex", "Fragment").
    /// </summary>
    public string? ShaderType { get; set; }

    /// <summary>
    /// Gets the compilation error log from the graphics driver.
    /// </summary>
    public string? CompilationLog { get; set; }
}
