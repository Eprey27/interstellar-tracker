namespace InterstellarTracker.WebUI.Models;

public record CelestialBodyDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public double? MassKg { get; init; }
    public double? RadiusMeters { get; init; }
}

public record CelestialBodyPositionDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required PositionDto Position { get; init; }
    public required VelocityDto Velocity { get; init; }
    public required DateTime CalculationDate { get; init; }
}

public record InterstellarObjectDto
{
    public required string Id { get; init; }
    public required string Designation { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset DiscoveryDate { get; init; }
    public required double Eccentricity { get; init; }
}

public record InterstellarObjectPositionDto
{
    public required string Id { get; init; }
    public required string Designation { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset DiscoveryDate { get; init; }
    public required string Discoverer { get; init; }
    public required double EstimatedDiameterMeters { get; init; }
    public required PositionDto Position { get; init; }
    public required VelocityDto Velocity { get; init; }
    public required DateTime CalculationDate { get; init; }
    public required double Eccentricity { get; init; }
}

public record PositionDto
{
    public required double X { get; init; }
    public required double Y { get; init; }
    public required double Z { get; init; }
    public required double Magnitude { get; init; }
}

public record VelocityDto
{
    public required double X { get; init; }
    public required double Y { get; init; }
    public required double Z { get; init; }
    public required double Magnitude { get; init; }
}
