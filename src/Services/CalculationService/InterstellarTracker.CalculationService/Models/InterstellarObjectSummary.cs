namespace InterstellarTracker.CalculationService.Models;

/// <summary>
/// Summary DTO for interstellar object list
/// </summary>
public record InterstellarObjectSummary
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Official designation (e.g., 2I/2019 Q4)
    /// </summary>
    public required string Designation { get; init; }

    /// <summary>
    /// Common name (e.g., Borisov)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Discovery date
    /// </summary>
    public required DateTimeOffset DiscoveryDate { get; init; }

    /// <summary>
    /// Orbital eccentricity (> 1 for hyperbolic)
    /// </summary>
    public required double Eccentricity { get; init; }
}
