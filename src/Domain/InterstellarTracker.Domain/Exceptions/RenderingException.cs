using System;

namespace InterstellarTracker.Domain.Exceptions;

/// <summary>
/// Thrown when the graphics rendering pipeline encounters an error.
/// This represents an error in the 3D rendering system (linking, linking, etc.).
/// </summary>
public class RenderingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderingException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public RenderingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderingException"/> class with a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RenderingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the operation that failed (e.g., "shader linking", "buffer creation").
    /// </summary>
    public string? Operation { get; set; }

    /// <summary>
    /// Gets the error details from the graphics API.
    /// </summary>
    public string? ErrorDetails { get; set; }
}
