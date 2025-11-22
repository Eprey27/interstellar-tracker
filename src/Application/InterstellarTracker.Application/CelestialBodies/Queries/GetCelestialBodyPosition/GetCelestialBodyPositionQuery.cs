using InterstellarTracker.Application.Common.Models;
using InterstellarTracker.Domain.ValueObjects;
using MediatR;

namespace InterstellarTracker.Application.CelestialBodies.Queries.GetCelestialBodyPosition;

/// <summary>
/// Query to get the position of a celestial body at a specific time.
/// </summary>
/// <param name="BodyId">The unique identifier of the celestial body.</param>
/// <param name="JulianDate">The Julian Date at which to calculate the position.</param>
public record GetCelestialBodyPositionQuery(string BodyId, double JulianDate)
    : IRequest<Result<Vector3>>;
