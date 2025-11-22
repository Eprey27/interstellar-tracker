using InterstellarTracker.Application.Common.Interfaces;
using InterstellarTracker.CalculationService.Models;
using InterstellarTracker.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace InterstellarTracker.CalculationService.Controllers;

/// <summary>
/// API endpoints for interstellar object tracking
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InterstellarObjectsController : ControllerBase
{
    private readonly ICelestialBodyRepository _repository;
    private readonly ILogger<InterstellarObjectsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterstellarObjectsController"/> class
    /// </summary>
    public InterstellarObjectsController(
        ICelestialBodyRepository repository,
        ILogger<InterstellarObjectsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get information and position of an interstellar object
    /// </summary>
    /// <param name="id">Interstellar object identifier (e.g., "oumuamua", "borisov", "atlas")</param>
    /// <param name="date">Date for calculation (ISO 8601 format, optional - defaults to now)</param>
    /// <returns>Interstellar object information with position and velocity</returns>
    /// <response code="200">Calculation successful</response>
    /// <response code="404">Interstellar object not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InterstellarObjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InterstellarObjectResponse>> GetInterstellarObject(
        string id,
        [FromQuery] DateTime? date = null)
    {
        var calculationDate = date ?? DateTime.UtcNow;

        _logger.LogInformation(
            "Retrieving interstellar object {ObjectId} at {Date}",
            id,
            calculationDate);

        var interstellarObject = await _repository.GetInterstellarObjectByIdAsync(id);

        if (interstellarObject == null)
        {
            _logger.LogWarning("Interstellar object {ObjectId} not found", id);

            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Interstellar Object Not Found",
                Detail = $"No interstellar object with ID '{id}' was found in the database."
            });
        }

        var julianDate = DateTimeToJulianDate(calculationDate);
        var position = interstellarObject.CalculatePosition(julianDate);
        var velocity = interstellarObject.CalculateVelocity(julianDate);

        var response = new InterstellarObjectResponse
        {
            Id = interstellarObject.Id,
            Designation = interstellarObject.Designation,
            Name = interstellarObject.Name,
            DiscoveryDate = interstellarObject.DiscoveryDate,
            Discoverer = interstellarObject.Discoverer,
            EstimatedDiameterMeters = interstellarObject.EstimatedDiameterMeters,
            Position = MapToPositionDto(position),
            Velocity = MapToVelocityDto(velocity),
            CalculationDate = calculationDate,
            Eccentricity = interstellarObject.OrbitalElements.Eccentricity
        };

        return Ok(response);
    }

    /// <summary>
    /// Get list of all known interstellar objects
    /// </summary>
    /// <returns>List of interstellar object identifiers and names</returns>
    /// <response code="200">List retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InterstellarObjectSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InterstellarObjectSummary>>> GetAllInterstellarObjects()
    {
        _logger.LogInformation("Retrieving all interstellar objects");

        var objects = await _repository.GetAllInterstellarObjectsAsync();

        var summaries = objects.Select(obj => new InterstellarObjectSummary
        {
            Id = obj.Id,
            Designation = obj.Designation,
            Name = obj.Name,
            DiscoveryDate = obj.DiscoveryDate,
            Eccentricity = obj.OrbitalElements.Eccentricity
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
    /// <param name="dateTime">DateTime to convert</param>
    /// <returns>Julian Date</returns>
    private static double DateTimeToJulianDate(DateTime dateTime)
    {
        // Convert to UTC if not already
        var utc = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

        // Julian Date calculation
        // JD = 367*Y - INT(7*(Y + INT((M+9)/12))/4) + INT(275*M/9) + D + 1721013.5 + UT/24
        int year = utc.Year;
        int month = utc.Month;
        int day = utc.Day;

        double a = Math.Floor((14.0 - month) / 12.0);
        double y = year + 4800 - a;
        double m = month + 12 * a - 3;

        double jdn = day + Math.Floor((153 * m + 2) / 5.0) + 365 * y + Math.Floor(y / 4.0) - Math.Floor(y / 100.0) + Math.Floor(y / 400.0) - 32045;
        double fraction = (utc.Hour - 12.0) / 24.0 + utc.Minute / 1440.0 + utc.Second / 86400.0 + utc.Millisecond / 86400000.0;

        return jdn + fraction;
    }
}

/// <summary>
/// Summary information for an interstellar object
/// </summary>
public record InterstellarObjectSummary
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Official designation
    /// </summary>
    public required string Designation { get; init; }

    /// <summary>
    /// Common name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Discovery date
    /// </summary>
    public required DateTimeOffset DiscoveryDate { get; init; }

    /// <summary>
    /// Orbital eccentricity
    /// </summary>
    public required double Eccentricity { get; init; }
}
