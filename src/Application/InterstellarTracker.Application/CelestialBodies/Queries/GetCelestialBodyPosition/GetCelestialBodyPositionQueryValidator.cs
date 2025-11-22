using FluentValidation;

namespace InterstellarTracker.Application.CelestialBodies.Queries.GetCelestialBodyPosition;

/// <summary>
/// Validator for GetCelestialBodyPositionQuery.
/// Ensures that the query parameters are valid.
/// </summary>
public class GetCelestialBodyPositionQueryValidator
    : AbstractValidator<GetCelestialBodyPositionQuery>
{
    // J2000 epoch (January 1, 2000, 12:00 TT)
    private const double J2000Epoch = 2451545.0;

    // Reasonable date range: 100 years before/after J2000
    private const double MinJulianDate = 2415020.5; // 1900-01-01
    private const double MaxJulianDate = 2488069.5; // 2100-01-01

    public GetCelestialBodyPositionQueryValidator()
    {
        RuleFor(x => x.BodyId)
            .NotEmpty()
            .WithMessage("Body ID is required.")
            .MaximumLength(100)
            .WithMessage("Body ID must not exceed 100 characters.");

        RuleFor(x => x.JulianDate)
            .GreaterThan(MinJulianDate)
            .WithMessage($"Julian Date must be after {MinJulianDate} (year 1900).")
            .LessThan(MaxJulianDate)
            .WithMessage($"Julian Date must be before {MaxJulianDate} (year 2100).");
    }
}
