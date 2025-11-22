using InterstellarTracker.Application.CelestialBodies.Queries.GetCelestialBodyPosition;
using InterstellarTracker.Application.Common.Interfaces;
using InterstellarTracker.CalculationService.Models;
using InterstellarTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InterstellarTracker.CalculationService.Controllers;

/// <summary>
/// API endpoints for celestial body calculations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CelestialBodiesController : ControllerBase
{
    private readonly ICelestialBodyRepository _repository;
    private readonly ILogger<CelestialBodiesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CelestialBodiesController"/> class
    /// </summary>
    public CelestialBodiesController(ICelestialBodyRepository repository, ILogger<CelestialBodiesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get the position of a celestial body at a specific date
    /// </summary>
    /// <param name="id">Celestial body identifier (e.g., "earth", "mars", "sun")</param>
    /// <param name="date">Date for calculation (ISO 8601 format, optional - defaults to now)</param>
    /// <returns>Position and velocity information</returns>
    /// <response code="200">Calculation successful</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="404">Celestial body not found</response>
    [HttpGet("{id}/position")]
    [ProducesResponseType(typeof(CelestialBodyPositionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CelestialBodyPositionResponse>> GetPosition(
        string id,
        [FromQuery] DateTime? date = null)
    {
        var calculationDate = date ?? DateTime.UtcNow;

        _logger.LogInformation(
            "Calculating position for celestial body {BodyId} at {Date}",
            id,
            calculationDate);

        var body = await _repository.GetByIdAsync(id);

        if (body == null)
        {
            _logger.LogWarning(
                "Celestial body {BodyId} not found",
                id);

            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Celestial Body Not Found",
                Detail = $"No celestial body with ID '{id}' was found in the database."
            });
        }

        var julianDate = ToJulianDate(calculationDate);
        var position = body.CalculatePosition(julianDate);
        var velocity = body.CalculateVelocity(julianDate);

        var response = new CelestialBodyPositionResponse
        {
            Id = body.Id,
            Name = body.Name,
            Type = body.Type.ToString(),
            Position = MapToPositionDto(position),
            Velocity = MapToVelocityDto(velocity),
            CalculationDate = calculationDate
        };

        return Ok(response);
    }

    /// <summary>
    /// Get list of all celestial bodies
    /// </summary>
    /// <returns>List of celestial body identifiers and names</returns>
    /// <response code="200">List retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CelestialBodySummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CelestialBodySummary>>> GetAllCelestialBodies()
    {
        _logger.LogInformation("Retrieving all celestial bodies");

        var bodies = await _repository.GetAllAsync();

        var summaries = bodies.Select(body => new CelestialBodySummary
        {
            Id = body.Id,
            Name = body.Name,
            Type = body.Type.ToString(),
            MassKg = body.MassKg,
            RadiusMeters = body.RadiusMeters
        });

        return Ok(summaries);
    }

    private static PositionDto MapToPositionDto(Vector3 vector)
    {
        return new PositionDto
        {
            X = vector.X,
            Y = vector.Y,
            Z = vector.Z,
            Magnitude = vector.Magnitude
        };
    }

    private static VelocityDto MapToVelocityDto(Vector3 vector)
    {
        return new VelocityDto
        {
            X = vector.X,
            Y = vector.Y,
            Z = vector.Z,
            Magnitude = vector.Magnitude
        };
    }

    /// <summary>
    /// Converts DateTime to Julian Date
    /// </summary>
    private static double ToJulianDate(DateTime dateTime)
    {
        // Convert to UTC if not already
        var utc = dateTime.ToUniversalTime();
        
        // Julian Date calculation
        int year = utc.Year;
        int month = utc.Month;
        int day = utc.Day;
        
        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }
        
        int a = year / 100;
        int b = 2 - a + (a / 4);
        
        double jd = Math.Floor(365.25 * (year + 4716)) +
                    Math.Floor(30.6001 * (month + 1)) +
                    day + b - 1524.5;
        
        // Add time of day
        jd += (utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0) / 24.0;
        
        return jd;
    }
}

/// <summary>
/// Summary information for a celestial body
/// </summary>
public record CelestialBodySummary
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type (Star, Planet, Comet, etc.)
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Mass in kilograms
    /// </summary>
    public required double MassKg { get; init; }

    /// <summary>
    /// Radius in meters
    /// </summary>
    public required double RadiusMeters { get; init; }
}
