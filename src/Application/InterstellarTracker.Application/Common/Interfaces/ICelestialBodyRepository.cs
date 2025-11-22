using InterstellarTracker.Domain.Entities;

namespace InterstellarTracker.Application.Common.Interfaces;

/// <summary>
/// Repository interface for accessing celestial body data.
/// Implementation will be in the Infrastructure layer.
/// </summary>
public interface ICelestialBodyRepository
{
    /// <summary>
    /// Gets a celestial body by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the celestial body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The celestial body if found, null otherwise.</returns>
    Task<CelestialBody?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all celestial bodies in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of all celestial bodies.</returns>
    Task<IEnumerable<CelestialBody>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an interstellar object by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the interstellar object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The interstellar object if found, null otherwise.</returns>
    Task<InterstellarObject?> GetInterstellarObjectByIdAsync(string id, CancellationToken cancellationToken = default);
}
