using InterstellarTracker.Application.CelestialBodies.Queries.GetCelestialBodyPosition;

namespace InterstellarTracker.Application.Tests.CelestialBodies.Queries;

/// <summary>
/// Unit tests for GetCelestialBodyPositionQueryValidator.
/// </summary>
public class GetCelestialBodyPositionQueryValidatorTests
{
    private readonly GetCelestialBodyPositionQueryValidator _validator;

    public GetCelestialBodyPositionQueryValidatorTests()
    {
        _validator = new GetCelestialBodyPositionQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_ReturnsValid()
    {
        // Arrange
        var query = new GetCelestialBodyPositionQuery("earth", 2451545.0);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyBodyId_ReturnsInvalid(string bodyId)
    {
        // Arrange
        var query = new GetCelestialBodyPositionQuery(bodyId, 2451545.0);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(query.BodyId));
    }

    [Fact]
    public void Validate_NullBodyId_ReturnsInvalid()
    {
        // Arrange
        var query = new GetCelestialBodyPositionQuery(null!, 2451545.0);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(query.BodyId));
    }

    [Fact]
    public void Validate_BodyIdTooLong_ReturnsInvalid()
    {
        // Arrange
        var longBodyId = new string('a', 101); // 101 characters
        var query = new GetCelestialBodyPositionQuery(longBodyId, 2451545.0);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(query.BodyId));
    }

    [Fact]
    public void Validate_JulianDateTooEarly_ReturnsInvalid()
    {
        // Arrange (before 1900)
        var query = new GetCelestialBodyPositionQuery("earth", 2400000.0);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(query.JulianDate));
    }

    [Fact]
    public void Validate_JulianDateTooLate_ReturnsInvalid()
    {
        // Arrange (after 2100)
        var query = new GetCelestialBodyPositionQuery("earth", 2500000.0);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(query.JulianDate));
    }

    [Theory]
    [InlineData(2451545.0)] // J2000 epoch (Jan 1, 2000)
    [InlineData(2415021.0)] // Jan 2, 1900 (after minimum boundary)
    [InlineData(2488068.0)] // Just before 2100 (before maximum boundary)
    [InlineData(2460000.0)] // Modern date (around 2023)
    public void Validate_ValidJulianDates_ReturnsValid(double julianDate)
    {
        // Arrange
        var query = new GetCelestialBodyPositionQuery("test-body", julianDate);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }
}
