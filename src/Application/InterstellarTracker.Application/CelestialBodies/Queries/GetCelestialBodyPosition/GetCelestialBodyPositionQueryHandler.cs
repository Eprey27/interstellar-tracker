using InterstellarTracker.Application.Common.Interfaces;
using InterstellarTracker.Application.Common.Models;
using InterstellarTracker.Domain.ValueObjects;
using MediatR;

namespace InterstellarTracker.Application.CelestialBodies.Queries.GetCelestialBodyPosition;

/// <summary>
/// Handler for GetCelestialBodyPositionQuery.
/// Calculates the position of a celestial body at a specific Julian Date.
/// </summary>
public class GetCelestialBodyPositionQueryHandler
    : IRequestHandler<GetCelestialBodyPositionQuery, Result<Vector3>>
{
    private readonly ICelestialBodyRepository _repository;

    public GetCelestialBodyPositionQueryHandler(ICelestialBodyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the query by retrieving the celestial body and calculating its position.
    /// </summary>
    public async Task<Result<Vector3>> Handle(
        GetCelestialBodyPositionQuery request,
        CancellationToken cancellationToken)
    {
        // First try to get as a regular celestial body
        var body = await _repository.GetByIdAsync(request.BodyId, cancellationToken);

        // If not found, try as an interstellar object
        if (body == null)
        {
            var interstellarObject = await _repository.GetInterstellarObjectByIdAsync(
                request.BodyId,
                cancellationToken);

            if (interstellarObject == null)
            {
                return Result<Vector3>.Failure($"Celestial body with ID '{request.BodyId}' not found.");
            }

            // Calculate position for interstellar object
            var position = interstellarObject.CalculatePosition(request.JulianDate);
            return Result<Vector3>.Success(position);
        }

        // Calculate position for regular celestial body
        var bodyPosition = body.CalculatePosition(request.JulianDate);
        return Result<Vector3>.Success(bodyPosition);
    }
}
